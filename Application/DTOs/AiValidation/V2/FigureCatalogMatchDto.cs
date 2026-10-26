namespace Application.DTOs.AiValidation.V2;

/// <summary>Preserves the catalog row supporting a semantic resolution, without storing database credentials or file data.</summary>
/// <param name="Id">The actual catalog ID found during this lookup, not a fixed semantic identifier.</param>
/// <param name="Code">The catalog business code as read.</param>
/// <param name="Description">The catalog description as read.</param>
/// <param name="IsActive">The recorded activity flag; inactive historical references are not silently discarded.</param>
public sealed record FigureCatalogMatchDto(int Id, string? Code, string? Description, bool? IsActive);
