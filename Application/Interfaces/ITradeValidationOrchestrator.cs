using Application.DTOs.AiValidation;

namespace Application.Interfaces;

public interface ITradeValidationOrchestrator
{
    Task<AiValidationResultDto> ValidateAsync(
        CreateAiValidationDto request,
        IReadOnlyList<AiValidationImageInputDto> images,
        CancellationToken cancellationToken);
}
