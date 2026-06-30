using Application.DTOs.AiValidation;
using Application.DTOs.AiProviders;
using Application.Interfaces;

namespace Infrastructure.ArtificialIntelligence;

public sealed class MiniMaxVisionClient : AiVisionClientBase
{
    private const string Provider = "MiniMax";

    public MiniMaxVisionClient(
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

    protected override string ExtractModelContent(string responseJson)
    {
        return ExtractChatCompletionContent(responseJson);
    }
}
