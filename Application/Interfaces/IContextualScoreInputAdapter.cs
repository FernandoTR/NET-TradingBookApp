using Application.DTOs.AiValidation;
using Application.DTOs.AiValidation.V2;
using Infrastructure;

namespace Application.Interfaces;

/// <summary>Adapts recorded legacy data without treating catalog selections as complete structural evidence.</summary>
public interface IContextualScoreInputAdapter
{
    /// <summary>Adapts an order while preserving missing base context, structures, convergences and factor reviews.</summary>
    /// <param name="order">The source order; its manual Scenery and original score are not calculation inputs.</param>
    /// <param name="cancellationToken">Cancellation for catalog lookups.</param>
    /// <returns>Only the contextual information supported by the recorded data.</returns>
    Task<ContextualScoreInputDto> FromOrderAsync(Order order, CancellationToken cancellationToken);

    /// <summary>Adapts a normalized legacy extraction independently of the provider's unchanged extraction schema.</summary>
    /// <param name="setup">The normalized legacy setup.</param>
    /// <param name="cancellationToken">Cancellation for catalog lookups.</param>
    /// <returns>The available figure and direction, leaving unrecorded contextual evidence unknown.</returns>
    Task<ContextualScoreInputDto> FromValidationAsync(NormalizedTradeSetupDto setup, CancellationToken cancellationToken);

    /// <summary>Adapts a persisted AI validation while retaining whether its figure came from AI or human confirmation.</summary>
    /// <param name="validation">The validation; images and provider response bodies are not included in score evidence.</param>
    /// <param name="cancellationToken">Cancellation for catalog lookups.</param>
    /// <returns>The available figure and direction, leaving unrecorded structural data unknown.</returns>
    Task<ContextualScoreInputDto> FromValidationAsync(AiTradeValidation validation, CancellationToken cancellationToken);
}
