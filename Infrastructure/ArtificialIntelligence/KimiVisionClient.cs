using Application.Common;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.ArtificialIntelligence;

public sealed class KimiVisionClient : AiVisionClientBase
{
    private const string Provider = "Kimi";

    public KimiVisionClient(
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
            model = ModelName,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = $"{PromptTemplateProvider.GetPrompt()}\nUse schema version {SchemaProvider.Version}. The required JSON schema is: {SchemaProvider.GetSchema()}"
                },
                new
                {
                    role = "user",
                    content = userContent
                }
            },
            response_format = new
            {
                type = "json_object"
            },
            temperature = 0,
            stream = false
        };
    }

    protected override string ExtractModelContent(string responseJson)
    {
        return ExtractChatCompletionContent(responseJson);
    }
}
