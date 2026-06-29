using Application.DTOs.AiValidation;
using Infrastructure;

namespace Application.Interfaces;

public interface IAiTradeValidationRepository
{
    Task<int> SaveCompletedAsync(AiTradeValidation validation, IEnumerable<AiTradeValidationRule> rules, CancellationToken cancellationToken);

    Task<AiTradeValidation?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AiTradeValidation>> GetByUserAsync(string userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AiTradeValidation>> GetCompletedByUserAsync(string userId, CancellationToken cancellationToken);

    Task<bool> ConfirmAsync(ConfirmedAiValidationDto confirmation, CancellationToken cancellationToken);

    Task<bool> LinkOrderAsync(int validationId, int orderId, string userId, CancellationToken cancellationToken);
}
