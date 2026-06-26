using Application.DTOs.AiValidation;

namespace Application.Interfaces;

public interface IAiVisionClient
{
    string ProviderName { get; }

    string ModelName { get; }

    Task<AiVisionExtractionDto> ExtractSetupAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        CancellationToken cancellationToken);
}
