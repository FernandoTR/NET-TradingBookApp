using Application.DTOs.AiValidation;

namespace Application.Interfaces;

public interface ITradeSetupNormalizer
{
    Task<NormalizedTradeSetupDto> NormalizeAsync(
        CreateAiValidationDto request,
        AiVisionExtractionDto extraction,
        CancellationToken cancellationToken);
}
