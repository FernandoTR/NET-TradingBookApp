using Application.DTOs.AiValidation;
using Application.DTOs.AiProviders;

namespace Application.Interfaces;

public interface IAiVisionClient
{
    string ProviderName { get; }

    string PromptVersion { get; }

    string SchemaVersion { get; }

    Task<AiVisionExtractionDto> ExtractSetupAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        AiProviderRuntimeConfiguration configuration,
        CancellationToken cancellationToken);
}
