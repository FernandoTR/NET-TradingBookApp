using Application.Common;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.ArtificialIntelligence;

public sealed class MiniMaxVisionClient : AiVisionClientBase
{
    private const string Provider = "MiniMax";

    public MiniMaxVisionClient(
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
