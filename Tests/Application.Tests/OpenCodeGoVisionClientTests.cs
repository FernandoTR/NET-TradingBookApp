using System.Net;
using System.Text;
using System.Text.Json;
using Application.DTOs.AiProviders;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.ArtificialIntelligence;

namespace Application.Tests;

public class OpenCodeGoVisionClientTests
{
    [Fact]
    public async Task ExtractSetupAsync_WithOpenAiChatCompletions_BuildsChatCompletionsRequest()
    {
        string? requestJson = null;

        var client = CreateClient(async (request, cancellationToken) =>
        {
            requestJson = await request.Content!.ReadAsStringAsync(cancellationToken);
            return CreateChatCompletionsResponse();
        });

        await ExecuteWithApiKeyForResultAsync(apiKeyName => client.ExtractSetupAsync(
            CreateRequest(),
            CreateImages(),
            CreateConfiguration(apiKeyName, "OpenAiChatCompletions", "https://opencode.test/v1/chat/completions"),
            CancellationToken.None));

        Assert.NotNull(requestJson);
        Assert.DoesNotContain("sk-test-key-value", requestJson);

        using var document = JsonDocument.Parse(requestJson);
        var root = document.RootElement;

        Assert.Equal("opencode-go-test", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());
        Assert.Equal("json_schema", root.GetProperty("response_format").GetProperty("type").GetString());

        var messages = root.GetProperty("messages");
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());

        var userContent = messages[1].GetProperty("content");
        Assert.Equal("text", userContent[0].GetProperty("type").GetString());
        Assert.Equal("image_url", userContent[1].GetProperty("type").GetString());
        Assert.StartsWith("data:image/png;base64,", userContent[1].GetProperty("image_url").GetProperty("url").GetString());
    }

    [Fact]
    public async Task ExtractSetupAsync_WithAnthropicMessages_BuildsMessagesRequest()
    {
        string? requestJson = null;

        var client = CreateClient(async (request, cancellationToken) =>
        {
            requestJson = await request.Content!.ReadAsStringAsync(cancellationToken);
            return CreateAnthropicMessagesResponse();
        });

        await ExecuteWithApiKeyForResultAsync(apiKeyName => client.ExtractSetupAsync(
            CreateRequest(),
            CreateImages(),
            CreateConfiguration(apiKeyName, "AnthropicMessages", "https://opencode.test/v1/messages"),
            CancellationToken.None));

        Assert.NotNull(requestJson);
        Assert.DoesNotContain("sk-test-key-value", requestJson);

        using var document = JsonDocument.Parse(requestJson);
        var root = document.RootElement;

        Assert.Equal("opencode-go-test", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());
        Assert.Equal(2048, root.GetProperty("max_tokens").GetInt32());
        Assert.Contains("ai-trade-validation-schema-v1", root.GetProperty("system").GetString());

        var userContent = root.GetProperty("messages")[0].GetProperty("content");
        Assert.Equal("text", userContent[0].GetProperty("type").GetString());
        Assert.Equal("image", userContent[1].GetProperty("type").GetString());

        var imageSource = userContent[1].GetProperty("source");
        Assert.Equal("base64", imageSource.GetProperty("type").GetString());
        Assert.Equal("image/png", imageSource.GetProperty("media_type").GetString());
        Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("fake png bytes")), imageSource.GetProperty("data").GetString());
        Assert.DoesNotContain("data:image", imageSource.GetProperty("data").GetString());
    }

    [Fact]
    public async Task ExtractSetupAsync_WithUnsupportedApiProtocol_FailsBeforeHttpSend()
    {
        var sendCount = 0;
        var client = CreateClient((_, _) =>
        {
            sendCount++;
            return Task.FromResult(CreateChatCompletionsResponse());
        });

        var exception = await ExecuteWithApiKeyAsync(apiKeyName => client.ExtractSetupAsync(
            CreateRequest(),
            CreateImages(),
            CreateConfiguration(apiKeyName, "UnsupportedProtocol", "https://opencode.test/v1/unsupported"),
            CancellationToken.None));

        Assert.Equal("unsupported_api_protocol", exception.ErrorCode);
        Assert.Equal(0, sendCount);
    }

    private static OpenCodeGoVisionClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        return new OpenCodeGoVisionClient(
            new HttpClient(new StubHttpMessageHandler(handler)),
            new PromptTemplateProvider(),
            new AiStructuredOutputSchemaProvider(),
            new NullLogService());
    }

    private static async Task<T> ExecuteWithApiKeyForResultAsync<T>(Func<string, Task<T>> action)
    {
        var apiKeyName = $"{nameof(OpenCodeGoVisionClientTests)}_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(apiKeyName, "sk-test-key-value");

        try
        {
            return await action(apiKeyName);
        }
        finally
        {
            Environment.SetEnvironmentVariable(apiKeyName, null);
        }
    }

    private static async Task<AiProviderException> ExecuteWithApiKeyAsync(Func<string, Task> action)
    {
        var apiKeyName = $"{nameof(OpenCodeGoVisionClientTests)}_{Guid.NewGuid():N}";
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

    private static AiProviderRuntimeConfiguration CreateConfiguration(
        string apiKeyName,
        string apiProtocol,
        string endpoint)
    {
        return new AiProviderRuntimeConfiguration
        {
            ProviderName = "OpenCodeGo",
            ModelName = "opencode-go-test",
            Endpoint = endpoint,
            ApiProtocol = apiProtocol,
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
            UserComment = "request comment"
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
                Content = new MemoryStream(Encoding.UTF8.GetBytes("fake png bytes"))
            }
        ];
    }

    private static HttpResponseMessage CreateChatCompletionsResponse()
    {
        var extractionJson = CreateExtractionJson();
        var responseJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = extractionJson
                    }
                }
            }
        });

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage CreateAnthropicMessagesResponse()
    {
        var extractionJson = CreateExtractionJson();
        var responseJson = JsonSerializer.Serialize(new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = extractionJson
                }
            }
        });

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };
    }

    private static string CreateExtractionJson()
    {
        return JsonSerializer.Serialize(new
        {
            triggerId = 1,
            sceneryId = 1,
            figureId = 1,
            frameId = 1,
            stageId = 1,
            locationType = 1,
            confirmationType = 1,
            isTrendAligned = true,
            isPivotZone = false,
            visualConfidence = 0.75
        });
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

    private sealed class NullLogService : ILogService
    {
        public void ErrorLog(string methodName, Exception exception)
        {
        }

        public void ErrorLog(string methodName, string message, string details)
        {
        }

        public void ActivityLog(string userId, string eventType, string description)
        {
        }
    }
}
