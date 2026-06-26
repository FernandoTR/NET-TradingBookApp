using System.Text.Json;
using Application.Common;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.ArtificialIntelligence;

public sealed class OpenAiVisionClient : AiVisionClientBase
{
    private const string Provider = "OpenAI";

    public OpenAiVisionClient(
        HttpClient httpClient,
        PromptTemplateProvider promptTemplateProvider,
        AiStructuredOutputSchemaProvider schemaProvider,
        IOptions<AiProviderOptions> options,
        ILogService logService)
        : base(Provider, httpClient, promptTemplateProvider, schemaProvider, options, logService)
    {
    }

    protected override object BuildProviderRequest(CreateAiValidationDto request, IReadOnlyList<AiVisionImagePayload> images)
    {
        var content = new List<object>
        {
            new
            {
                type = "input_text",
                text = BuildTradeContext(request)
            }
        };

        foreach (var image in images)
        {
            content.Add(new
            {
                type = "input_image",
                image_url = image.DataUri,
                detail = "high"
            });
        }

        return new
        {
            model = ModelName,
            instructions = PromptTemplateProvider.GetPrompt(),
            input = new[]
            {
                new
                {
                    role = "user",
                    content
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = SchemaProvider.Version,
                    schema = GetSchemaJsonElement(),
                    strict = true
                }
            }
        };
    }

    protected override string ExtractModelContent(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? throw new JsonException("OpenAI response output_text was empty.");
        }

        if (root.TryGetProperty("output", out var output))
        {
            foreach (var outputItem in output.EnumerateArray())
            {
                if (!outputItem.TryGetProperty("content", out var content))
                {
                    continue;
                }

                foreach (var contentItem in content.EnumerateArray())
                {
                    if (contentItem.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    {
                        return text.GetString() ?? throw new JsonException("OpenAI response text was empty.");
                    }
                }
            }
        }

        return ExtractChatCompletionContent(responseJson);
    }
}
