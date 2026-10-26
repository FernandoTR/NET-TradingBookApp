using Application.DTOs.AiValidation.V2;

namespace Application.Interfaces;

/// <summary>Resolves supplied figure observations against the actual catalog without using stage or IsPivotZone as proxies.</summary>
public interface IContextualFigureResolver
{
    /// <summary>Identifies a unique consistent catalog row and its verified semantic concept.</summary>
    /// <param name="reference">The supplied concept, label and independently available catalog ID.</param>
    /// <param name="cancellationToken">Cancellation for the lookup.</param>
    /// <returns>The source observation, matching catalog evidence, mapping version and resolution reasons.</returns>
    Task<FigureResolutionResultDto> ResolveAsync(CatalogReferenceDto reference, CancellationToken cancellationToken);
}
