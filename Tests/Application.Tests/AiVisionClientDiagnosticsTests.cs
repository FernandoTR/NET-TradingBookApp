using System.Net;
using System.Text;
using System.Text.Json;
using Application.DTOs.AiProviders;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.ArtificialIntelligence;

namespace Application.Tests;

public class AiVisionClientDiagnosticsTests
{
    [Fact]
    public async Task ExtractSetupAsync_WhenProviderReturns429Json_LogsStructuredProviderDiagnostics()
    {
        var logService = new CapturingLogService();
        var response = new HttpResponseMessage((HttpStatusCode)429)
        {
            ReasonPhrase = "Too Many Requests",
            Content = new StringContent(
                """
                {
                  "error": {
                    "message": "Rate limit reached for tokens.",
                    "type": "rate_limit_exceeded",
                    "code": "tokens",
                    "param": null
                  }
                }
                """,
                Encoding.UTF8,
                "application/json")
        };
        response.Headers.TryAddWithoutValidation("retry-after", "2");
        response.Headers.TryAddWithoutValidation("x-request-id", "req_test_123");
        response.Headers.TryAddWithoutValidation("x-ratelimit-limit-tokens", "1000");

        var client = CreateClient(logService, (_, _) => Task.FromResult(response));

        var exception = await ExecuteWithApiKeyAsync(
            apiKeyName => client.ExtractSetupAsync(CreateRequest(), CreateImages(), CreateConfiguration(apiKeyName), CancellationToken.None));

        Assert.Equal("rate_limited", exception.ErrorCode);
        Assert.Equal((HttpStatusCode)429, exception.StatusCode);
        Assert.Equal("tokens", exception.ProviderErrorCode);
        Assert.Equal("rate_limit_exceeded", exception.ProviderErrorType);
        Assert.Equal("Rate limit reached for tokens.", exception.ProviderErrorMessage);
        Assert.Equal("2", exception.RetryAfter);
        Assert.Equal("req_test_123", exception.RequestId);

        var log = Assert.Single(logService.ErrorDetails);
        Assert.Equal("ExtractSetupAsync", log.MethodName);
        Assert.Equal("AI provider extraction failed.", log.Message);
        Assert.Equal(exception.DiagnosticJson, log.Details);

        using var document = JsonDocument.Parse(log.Details);
        var root = document.RootElement;

        Assert.Equal("OpenAI", root.GetProperty("providerName").GetString());
        Assert.Equal("gpt-test", root.GetProperty("modelName").GetString());
        Assert.Equal("api.openai.test", root.GetProperty("endpointHost").GetString());
        Assert.Equal("/v1/responses", root.GetProperty("endpointPath").GetString());
        Assert.Equal(429, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Too Many Requests", root.GetProperty("reasonPhrase").GetString());
        Assert.Equal("Rate limit reached for tokens.", root.GetProperty("providerError").GetProperty("message").GetString());
        Assert.Equal("2", root.GetProperty("headers").GetProperty("retry-after")[0].GetString());
        Assert.Equal(1, root.GetProperty("imageSummary").GetProperty("count").GetInt32());
        Assert.Equal("high", root.GetProperty("imageSummary").GetProperty("detailValues")[0].GetString());
    }

    [Fact]
    public async Task ExtractSetupAsync_WhenProviderReturns429NonJson_LogsSanitizedBodySnippet()
    {
        var logService = new CapturingLogService();
        var response = new HttpResponseMessage((HttpStatusCode)429)
        {
            ReasonPhrase = "Too Many Requests",
            Content = new StringContent(
                "Too many requests for Bearer sk-testsecret12345 data:image/png;base64," + new string('A', 180),
                Encoding.UTF8,
                "text/plain")
        };

        var client = CreateClient(logService, (_, _) => Task.FromResult(response));

        var exception = await ExecuteWithApiKeyAsync(
            apiKeyName => client.ExtractSetupAsync(CreateRequest(), CreateImages(), CreateConfiguration(apiKeyName), CancellationToken.None));

        Assert.Equal("rate_limited", exception.ErrorCode);

        var log = Assert.Single(logService.ErrorDetails);
        using var document = JsonDocument.Parse(log.Details);
        var responseDiagnostic = document.RootElement.GetProperty("response");

        Assert.False(responseDiagnostic.GetProperty("isJson").GetBoolean());
        Assert.Contains("[redacted]", responseDiagnostic.GetProperty("bodySnippet").GetString());
        Assert.Contains("[redacted-data-uri]", responseDiagnostic.GetProperty("bodySnippet").GetString());
        Assert.DoesNotContain("sk-testsecret12345", log.Details);
        Assert.DoesNotContain("Bearer sk-", log.Details);
        Assert.DoesNotContain("data:image", log.Details);
        Assert.DoesNotContain("base64", log.Details);
    }

    [Fact]
    public async Task ExtractSetupAsync_WhenRequestTimesOut_LogsDiagnosticsWithoutResponseBody()
    {
        var logService = new CapturingLogService();
        var client = CreateClient(logService, (_, _) => throw new TaskCanceledException("simulated timeout"));

        var exception = await ExecuteWithApiKeyAsync(
            apiKeyName => client.ExtractSetupAsync(CreateRequest(), CreateImages(), CreateConfiguration(apiKeyName), CancellationToken.None));

        Assert.Equal("timeout", exception.ErrorCode);
        Assert.Null(exception.StatusCode);
        Assert.Null(exception.ProviderErrorCode);

        var log = Assert.Single(logService.ErrorDetails);
        using var document = JsonDocument.Parse(log.Details);
        var root = document.RootElement;

        Assert.Equal("timeout", root.GetProperty("failureKind").GetString());
        Assert.Equal("TaskCanceledException", root.GetProperty("exceptionType").GetString());
        Assert.False(root.TryGetProperty("response", out _));
        Assert.Equal(1, root.GetProperty("imageSummary").GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task ExtractSetupAsync_DiagnosticsDoNotIncludeApiKeyImageDataOrPrompt()
    {
        var logService = new CapturingLogService();
        var response = new HttpResponseMessage((HttpStatusCode)429)
        {
            Content = new StringContent(
                """
                {
                  "error": {
                    "message": "Rejected input Bearer sk-sensitive123456 and data:image/png;base64,AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                    "type": "rate_limit_exceeded",
                    "code": "requests"
                  }
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var client = CreateClient(logService, (_, _) => Task.FromResult(response));

        await ExecuteWithApiKeyAsync(
            apiKeyName => client.ExtractSetupAsync(CreateRequest(), CreateImages(), CreateConfiguration(apiKeyName), CancellationToken.None));

        var log = Assert.Single(logService.ErrorDetails);

        Assert.DoesNotContain("sk-sensitive123456", log.Details);
        Assert.DoesNotContain("data:image", log.Details);
        Assert.DoesNotContain("base64", log.Details);
        Assert.DoesNotContain("You are a trading setup vision extraction engine", log.Details);
        Assert.DoesNotContain("Trade setup input", log.Details);
        Assert.DoesNotContain("iVBOR", log.Details);
    }

    private static OpenAiVisionClient CreateClient(
        CapturingLogService logService,
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        return new OpenAiVisionClient(
            new HttpClient(new StubHttpMessageHandler(handler)),
            new PromptTemplateProvider(),
            new AiStructuredOutputSchemaProvider(),
            logService);
    }

    private static async Task<AiProviderException> ExecuteWithApiKeyAsync(
        Func<string, Task> action)
    {
        var apiKeyName = $"{nameof(AiVisionClientDiagnosticsTests)}_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(apiKeyName, "sk-test-key-value");

        try
        {
            return await Assert.ThrowsAsync<AiProviderException>(() => action(apiKeyName));
        }
        finally
        {
            Environment.SetEnvironmentVariable(apiKeyName, null);
        }
    }

    private static AiProviderRuntimeConfiguration CreateConfiguration(string apiKeyName)
    {
        return new AiProviderRuntimeConfiguration
        {
            ProviderName = "OpenAI",
            ModelName = "gpt-test",
            Endpoint = "https://api.openai.test/v1/responses?ignored=query",
            ApiKeyEnvironmentVariable = apiKeyName,
            SupportsVision = true,
            TimeoutSeconds = 60
        };
    }

    private static CreateAiValidationDto CreateRequest()
    {
        return new CreateAiValidationDto
        {
            UserId = "user-test",
            InstrumentId = 6,
            DirectionId = 1,
            EntryPrice = 0.7098m,
            StopLoss = 0.6915m,
            TakeProfit = 0.7281m,
            UserComment = "comment should not be logged"
        };
    }

    private static IReadOnlyList<AiValidationImageInputDto> CreateImages()
    {
        return
        [
            new AiValidationImageInputDto
            {
                OriginalFileName = "chart.png",
                ContentType = "image/png",
                FileSize = 13,
                FrameCode = "M15",
                ImageRole = TradingImageRole.MainTimeframe,
                SortOrder = 1,
                Comment = "image comment should not be logged",
                Content = new MemoryStream(Encoding.UTF8.GetBytes("fake png bytes"))
            }
        ];
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request, cancellationToken);
        }
    }

    private sealed class CapturingLogService : ILogService
    {
        public List<(string MethodName, string Message, string Details)> ErrorDetails { get; } = [];

        public void ErrorLog(string methodName, Exception exception)
        {
            ErrorDetails.Add((methodName, exception.Message, exception.ToString()));
        }

        public void ErrorLog(string methodName, string message, string details)
        {
            ErrorDetails.Add((methodName, message, details));
        }

        public void ActivityLog(string userId, string eventType, string description)
        {
        }
    }
}
