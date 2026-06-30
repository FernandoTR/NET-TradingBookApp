using Application.DTOs.AiValidation;
using Application.DTOs.AiProviders;
using Application.Interfaces;

namespace Infrastructure.ArtificialIntelligence;

public sealed class KimiVisionClient : AiVisionClientBase
{
    private const string Provider = "Kimi";

    public KimiVisionClient(
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
