using Application.DTOs.AiValidation;

namespace Application.Interfaces;

public interface IHistoricalEvidenceService
{
    Task<HistoricalEvidenceDto?> GetEvidenceAsync(
        NormalizedTradeSetupDto setup,
        CancellationToken cancellationToken);
}
