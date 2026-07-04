using System.Text.Json;
using Application.DTOs.AiProviders;
using Application.DTOs.AiValidation;
using Application.Interfaces;

namespace Infrastructure.ArtificialIntelligence;

public sealed class OpenCodeGoVisionClient : AiVisionClientBase
{
    private const string Provider = "OpenCodeGo";
    private const string OpenAiChatCompletionsProtocol = "OpenAiChatCompletions";
    private const string AnthropicMessagesProtocol = "AnthropicMessages";
    private const int AnthropicMaxTokens = 2048;

    public OpenCodeGoVisionClient(
        HttpClient httpClient,
        PromptTemplateProvider promptTemplateProvider,
        AiStructuredOutputSchemaProvider schemaProvider,
        ILogService logService)
        : base(Provider, httpClient, promptTemplateProvider, schemaProvider, logService)
    {
    }

    protected override object BuildProviderRequest(
        CreateAiValidationDto request,
        IReadOnlyList<AiVisionImagePayload> images,
        AiProviderRuntimeConfiguration configuration)
    {
        if (string.Equals(configuration.ApiProtocol, OpenAiChatCompletionsProtocol, StringComparison.OrdinalIgnoreCase))
        {
            return BuildOpenAiChatCompletionsRequest(request, images, configuration);
        }

        if (string.Equals(configuration.ApiProtocol, AnthropicMessagesProtocol, StringComparison.OrdinalIgnoreCase))
        {
            return BuildAnthropicMessagesRequest(request, images, configuration);
        }

        throw new AiProviderException(
            "unsupported_api_protocol",
            ProviderName,
            configuration.ModelName,
            $"OpenCode Go API protocol '{configuration.ApiProtocol}' is not supported.");
    }

    protected override string ExtractModelContent(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (root.TryGetProperty("choices", out _))
        {
            return ExtractChatCompletionContent(responseJson);
        }

        if (root.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    return text.GetString() ?? throw new JsonException("OpenCode Go response text was empty.");
                }
            }
        }

        throw new JsonException("OpenCode Go response did not contain supported model content.");
    }

    private object BuildOpenAiChatCompletionsRequest(
        CreateAiValidationDto request,
        IReadOnlyList<AiVisionImagePayload> images,
        AiProviderRuntimeConfiguration configuration)
    {
        var userContent = new List<object>
        {
            BuildChatTextPart(BuildTradeContext(request))
        };

        foreach (var image in images)
        {
            userContent.Add(BuildChatImagePart(image));
        }

        return new
        {
            model = configuration.ModelName,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = PromptTemplateProvider.GetPrompt()
                },
                new
                {
                    role = "user",
                    content = userContent
                }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = SchemaProvider.Version,
                    schema = GetSchemaJsonElement(),
                    strict = true
                }
            },
            stream = false
        };
    }

    private object BuildAnthropicMessagesRequest(
        CreateAiValidationDto request,
        IReadOnlyList<AiVisionImagePayload> images,
        AiProviderRuntimeConfiguration configuration)
    {
        var userContent = new List<object>
        {
            new
            {
                type = "text",
                text = BuildTradeContext(request)
            }
        };

        foreach (var image in images)
        {
            userContent.Add(new
            {
                type = "image",
                source = new
                {
                    type = "base64",
                    media_type = image.ContentType,
                    data = ExtractBase64Data(image.DataUri)
                }
            });
        }

        return new
        {
            model = configuration.ModelName,
            max_tokens = AnthropicMaxTokens,
            system = $"{PromptTemplateProvider.GetPrompt()}\nUse schema version {SchemaProvider.Version}. The required JSON schema is: {SchemaProvider.GetSchema()}",
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = userContent
                }
            },
            temperature = 0,
            stream = false
        };
    }

    private static string ExtractBase64Data(string dataUri)
    {
        var commaIndex = dataUri.IndexOf(',', StringComparison.Ordinal);
        if (commaIndex < 0 || commaIndex == dataUri.Length - 1)
        {
            throw new JsonException("Image payload data URI did not contain base64 data.");
        }

        return dataUri[(commaIndex + 1)..];
    }
}
