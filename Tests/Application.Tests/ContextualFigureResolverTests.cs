using System.Text.Json;
using Application.DTOs;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class ContextualFigureResolverTests
{
    [Theory]
    [InlineData("LP", null)]
    [InlineData(null, "LÍNEA PIVOTE")]
    [InlineData(null, "  línea   pivote  ")]
    [InlineData("PIVOT_LINE", "Línea pivote")]
    public async Task PivotLine_ResolvesVerifiedAliasesWithoutAFixedId(string? code, string? label)
    {
        var catalog = new FigureCatalog(Row(734, "LP", "LINEA PIVOTE"), Row(4, "LT", "LINEA TENDENCIA"));
        var resolver = new ContextualFigureResolver(catalog);

        var result = await resolver.ResolveAsync(Reference(code, label), CancellationToken.None);

        Assert.True(result.IsResolved);
        Assert.True(result.IsPivotLine);
        Assert.Equal(FigureConceptKind.PivotLine, result.Concept.Value);
        Assert.Equal(734, result.ResolvedReference!.NormalizedId.Value);
        Assert.Equal("LP", result.MatchedFigure!.Code);
        Assert.Equal("LINEA PIVOTE", result.MatchedFigure.Description);
        Assert.Equal(ContextualFigureResolver.MappingVersion, result.MappingVersion);
    }

    [Theory]
    [InlineData("CA", "CANAL ASCENDENTE", FigureConceptKind.AscendingChannel)]
    [InlineData("CD", "CANAL DESCENDENTE", FigureConceptKind.DescendingChannel)]
    [InlineData("CN", "CANAL NEUTRO", FigureConceptKind.NeutralChannel)]
    [InlineData("TD", "TRIANGULO DESCENDENTE", FigureConceptKind.DescendingTriangle)]
    [InlineData("TA", "TRIANGULO ASCENDENTE", FigureConceptKind.AscendingTriangle)]
    [InlineData("TS", "TRIANGULO SIMETRICO", FigureConceptKind.SymmetricTriangle)]
    [InlineData("TM", "TAZA Y MANGO", FigureConceptKind.CupAndHandle)]
    [InlineData("LT", "LINEA TENDENCIA", FigureConceptKind.TrendLine)]
    public async Task OtherVerifiedFigures_AreDistinguishedFromPivotLine(string code, string description, FigureConceptKind expected)
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(4, code, description)));

        var result = await resolver.ResolveAsync(Reference(null, null, 4), CancellationToken.None);

        Assert.True(result.IsResolved);
        Assert.False(result.IsPivotLine);
        Assert.Equal(expected, result.Concept.Value);
    }

    [Fact]
    public async Task RenamedCatalogCode_CanResolveThroughTheVerifiedDescription()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(91, "Z7", "LÍNEA PIVOTE")));

        var result = await resolver.ResolveAsync(Reference("PIVOT_LINE", null), CancellationToken.None);

        Assert.True(result.IsPivotLine);
        Assert.Equal(91, result.ResolvedReference!.NormalizedId.Value);
        Assert.Equal("Z7", result.MatchedFigure!.Code);
    }

    [Fact]
    public async Task DuplicateSemanticRows_RemainAmbiguousAndRetainBothCandidates()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(
            Row(10, "LP", "LINEA PIVOTE"), Row(20, "P2", "LÍNEA PIVOTE")));

        var result = await resolver.ResolveAsync(Reference("LP", null), CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Null(result.ResolvedReference);
        Assert.Equal(EvidenceAvailability.Unknown, result.Concept.Availability);
        Assert.Equal(new[] { 10, 20 }, result.Candidates.Select(candidate => candidate.Id));
        Assert.Contains("FIGURE_CATALOG_AMBIGUOUS", result.ReasonCodes);
    }

    [Fact]
    public async Task ExplicitConsistentId_CanDisambiguateEquivalentCatalogConcepts()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(
            Row(10, "LP", "LINEA PIVOTE"), Row(20, "P2", "LÍNEA PIVOTE")));

        var result = await resolver.ResolveAsync(Reference("LP", null, 20), CancellationToken.None);

        Assert.True(result.IsResolved);
        Assert.True(result.IsPivotLine);
        Assert.Equal(20, result.ResolvedReference!.NormalizedId.Value);
        Assert.Equal(2, result.Candidates.Count);
    }

    [Fact]
    public async Task ContradictoryIdAndLabel_DoNotSilentlyReplaceTheSuppliedId()
    {
        var reference = Reference(null, "Línea pivote", 2);
        var resolver = new ContextualFigureResolver(new FigureCatalog(
            Row(2, "CA", "CANAL ASCENDENTE"), Row(4, "LP", "LINEA PIVOTE")));

        var result = await resolver.ResolveAsync(reference, CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Null(result.ResolvedReference);
        Assert.Equal(2, result.SourceReference.NormalizedId.Value);
        Assert.Equal(new[] { 2, 4 }, result.Candidates.Select(candidate => candidate.Id));
        Assert.Contains("FIGURE_REFERENCE_CONFLICT", result.ReasonCodes);
    }

    [Fact]
    public async Task ContradictoryConceptAndLabel_RemainUnresolved()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(
            Row(2, "CA", "CANAL ASCENDENTE"), Row(4, "LP", "LINEA PIVOTE")));

        var result = await resolver.ResolveAsync(Reference("CA", "Línea pivote"), CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Contains("FIGURE_REFERENCE_CONFLICT", result.ReasonCodes);
    }

    [Fact]
    public async Task InconsistentCatalogSemantics_AreNotResolvedByChoosingCodeOverDescription()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(4, "LP", "CANAL ASCENDENTE")));

        var result = await resolver.ResolveAsync(Reference(null, null, 4), CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Contains("FIGURE_CATALOG_SEMANTIC_CONFLICT", result.ReasonCodes);
        Assert.Single(result.Candidates);
    }

    [Fact]
    public async Task MissingId_DoesNotFallBackToAnotherRowWithAMatchingLabel()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(4, "LP", "LINEA PIVOTE")));

        var result = await resolver.ResolveAsync(Reference(null, "Línea pivote", 999), CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.ResolvedReference);
        Assert.Equal(999, result.SourceReference.NormalizedId.Value);
        Assert.Contains("FIGURE_CATALOG_ID_NOT_FOUND", result.ReasonCodes);
    }

    [Fact]
    public async Task MissingPivotLine_DoesNotSubstituteAnUnrelatedFigure()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(4, "LT", "LINEA TENDENCIA")));

        var result = await resolver.ResolveAsync(Reference("LP", null), CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Contains("FIGURE_CATALOG_NOT_FOUND", result.ReasonCodes);
    }

    [Fact]
    public async Task UnmappedFigure_PreservesItsCatalogIdentityWithoutGuessingPivotSemantics()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(5, "ZP", "ZONA PIVOTE")));

        var result = await resolver.ResolveAsync(Reference(null, null, 5), CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Equal(5, result.ResolvedReference!.NormalizedId.Value);
        Assert.Equal("ZONA PIVOTE", result.MatchedFigure!.Description);
        Assert.Equal(EvidenceAvailability.Unknown, result.Concept.Availability);
        Assert.Contains("FIGURE_CONCEPT_NOT_MAPPED", result.ReasonCodes);
    }

    [Fact]
    public async Task SubstringMatch_IsNotTreatedAsAnExactPivotLineConcept()
    {
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(4, "LP", "LINEA PIVOTE")));

        var result = await resolver.ResolveAsync(Reference(null, "Confluencia de línea pivote"), CancellationToken.None);

        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Contains("FIGURE_CATALOG_NOT_FOUND", result.ReasonCodes);
    }

    [Theory]
    [InlineData(EvidenceAvailability.Unknown)]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public async Task UnavailableObservation_RetainsItsStateWithoutQueryingOrInventingAFigure(EvidenceAvailability availability)
    {
        var catalog = new FigureCatalog(Row(4, "LP", "LINEA PIVOTE"));
        var resolver = new ContextualFigureResolver(catalog);
        var reference = new CatalogReferenceDto(availability, null, null,
            new(availability, null, null, null, null, null, "NO_FIGURE"),
            null, null, null, "NO_FIGURE");

        var result = await resolver.ResolveAsync(reference, CancellationToken.None);

        Assert.Equal(0, catalog.Reads);
        Assert.False(result.IsResolved);
        Assert.Null(result.IsPivotLine);
        Assert.Equal(availability, result.Concept.Availability);
        Assert.Null(result.Concept.Value);
        Assert.Equal(reference, result.SourceReference);
    }

    [Fact]
    public async Task Normalization_PreservesOriginalObservationAndAddsItsOwnProvenance()
    {
        var observedAt = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var reference = Reference(null, "línea pivote") with
        {
            Source = ValidationSource.AiVision,
            OccurredAtUtc = observedAt,
            AvailableAtUtc = observedAt.AddMinutes(1)
        };
        var resolver = new ContextualFigureResolver(new FigureCatalog(Row(734, "LP", "LINEA PIVOTE")));
        var startedAt = DateTimeOffset.UtcNow;

        var result = await resolver.ResolveAsync(reference, CancellationToken.None);
        var restored = JsonSerializer.Deserialize<FigureResolutionResultDto>(JsonSerializer.Serialize(result))!;

        Assert.Equal(reference, restored.SourceReference);
        Assert.Equal(reference.ObservedLabel, restored.ResolvedReference!.ObservedLabel);
        Assert.Equal(reference.Source, restored.ResolvedReference.Source);
        Assert.Equal(reference.AvailableAtUtc, restored.ResolvedReference.AvailableAtUtc);
        Assert.Equal(ValidationSource.DeterministicRule, restored.ResolvedReference.NormalizedId.Source);
        Assert.Null(restored.ResolvedReference.NormalizedId.OccurredAtUtc);
        Assert.InRange(restored.ResolvedReference.NormalizedId.AvailableAtUtc!.Value, startedAt, DateTimeOffset.UtcNow);
        Assert.Equal(EvidenceAvailability.Unknown, reference.NormalizedId.Availability);
        Assert.Equal(result.Candidates, restored.Candidates);
        Assert.Equal(result.MappingVersion, restored.MappingVersion);
        Assert.True(restored.IsPivotLine);
    }

    [Fact]
    public async Task InactiveHistoricalRow_IsIdentifiedWithoutDiscardingItsActivityFlag()
    {
        var row = Row(734, "LP", "LINEA PIVOTE");
        row.IsActived = false;
        var resolver = new ContextualFigureResolver(new FigureCatalog(row));

        var result = await resolver.ResolveAsync(Reference(null, null, 734), CancellationToken.None);

        Assert.True(result.IsResolved);
        Assert.True(result.IsPivotLine);
        Assert.False(result.MatchedFigure!.IsActive);
    }

    [Fact]
    public async Task CatalogChanges_AreRecheckedOnAnExplicitNewResolution()
    {
        var row = Row(4, "LP", "LINEA PIVOTE");
        var catalog = new FigureCatalog(row);
        var resolver = new ContextualFigureResolver(catalog);
        var reference = Reference(null, null, 4);

        var original = await resolver.ResolveAsync(reference, CancellationToken.None);
        row.Code = "LT";
        row.Description = "LINEA TENDENCIA";
        var recalculated = await resolver.ResolveAsync(reference, CancellationToken.None);

        Assert.True(original.IsPivotLine);
        Assert.Equal("LP", original.MatchedFigure!.Code);
        Assert.False(recalculated.IsPivotLine);
        Assert.Equal("LT", recalculated.MatchedFigure!.Code);
        Assert.Equal(2, catalog.Reads);
    }

    [Fact]
    public async Task InvalidObservation_DoesNotQueryTheCatalog()
    {
        var catalog = new FigureCatalog();
        var reference = Reference(null, null);

        var result = await new ContextualFigureResolver(catalog).ResolveAsync(reference, CancellationToken.None);

        Assert.Equal(0, catalog.Reads);
        Assert.Contains("FIGURE_REFERENCE_INVALID", result.ReasonCodes);
    }

    [Fact]
    public async Task Cancellation_IsHonoredBeforeAndAfterTheCatalogRead()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var catalog = new FigureCatalog(Row(4, "LP", "LINEA PIVOTE"));
        var resolver = new ContextualFigureResolver(catalog);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => resolver.ResolveAsync(Reference("LP", null), cancelled.Token));
        Assert.Equal(0, catalog.Reads);

        using var duringRead = new CancellationTokenSource();
        catalog.OnRead = duringRead.Cancel;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => resolver.ResolveAsync(Reference("LP", null), duringRead.Token));
        Assert.Equal(1, catalog.Reads);
    }

    [Fact]
    public async Task TechnicalCatalogFailure_IsNotReportedAsConfirmedAbsence()
    {
        var catalog = new FigureCatalog { Failure = new InvalidOperationException("Catalog unavailable.") };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ContextualFigureResolver(catalog).ResolveAsync(Reference("LP", null), CancellationToken.None));
    }

    [Fact]
    public async Task ApplicationRegistration_ResolvesTheNewServiceWithTheExistingCatalogContract()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddApplicationServices();
        builder.Services.AddSingleton<ICatFigureService>(new FigureCatalog(Row(734, "LP", "LINEA PIVOTE")));
        using var provider = builder.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var resolver = scope.ServiceProvider.GetRequiredService<IContextualFigureResolver>();
        var result = await resolver.ResolveAsync(Reference("LP", null), CancellationToken.None);

        Assert.True(result.IsPivotLine);
    }

    private static CatalogReferenceDto Reference(string? code, string? label, int? id = null) => new(
        EvidenceAvailability.Present, code, label,
        id.HasValue
            ? new(EvidenceAvailability.Present, id.Value, ValidationSource.UserInput, null, null, null, null)
            : new(EvidenceAvailability.Unknown, null, null, null, null, null, "CATALOG_NOT_NORMALIZED"),
        ValidationSource.UserInput, null, null, null);

    private static CatFigure Row(int id, string code, string description) => new()
    {
        Id = id,
        Code = code,
        Description = description,
        IsActived = true
    };

    private sealed class FigureCatalog(params CatFigure[] figures) : ICatFigureService
    {
        public int Reads { get; private set; }
        public Action? OnRead { get; set; }
        public Exception? Failure { get; init; }

        public Task<IEnumerable<CatFigure>> GetAllAsync()
        {
            Reads++;
            OnRead?.Invoke();
            if (Failure is not null)
            {
                throw Failure;
            }

            return Task.FromResult<IEnumerable<CatFigure>>(figures);
        }

        public Task<CatFigure?> GetByIdAsync(int id) => Task.FromResult(figures.SingleOrDefault(figure => figure.Id == id));
        public Task<bool> AddAsync(CatFigure entity) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id) => throw new NotSupportedException();
        public Task<bool> UpdateAsync(CatFigure entity) => throw new NotSupportedException();
        public Task<List<GetTBAnalyticsFigureDto>> GetTBAnalyticsFigureAsync(ParametersTBAnalyticsDto parameters) => throw new NotSupportedException();
    }
}
