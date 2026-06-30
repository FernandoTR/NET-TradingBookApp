using Application.DTOs.AiValidation;
using Application.DTOs.AiProviders;
using Application.Interfaces;

namespace Infrastructure.ArtificialIntelligence;

public sealed class GlmVisionClient : AiVisionClientBase
{
    private const string Provider = "GLM";

    public GlmVisionClient(
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
                    content = $"{PromptTemplateProvider.GetPrompt()}\nReturn JSON that matches schema {SchemaProvider.Version}: {SchemaProvider.GetSchema()}"
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
            temperature = 0.01,
            top_p = 0.1,
            stream = false
        };
    }

    protected override string ExtractModelContent(string responseJson)
    {
        return ExtractChatCompletionContent(responseJson);
    }
}
