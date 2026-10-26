using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.DTOs.AiValidation.V2;
using Domain.Enums;

namespace Application.Tests;

public class MapEvidenceContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset SetupAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");

    [Fact]
    public void StageOneZone_RoundTripsWithoutAFigureOrAssumingConfirmation()
    {
        var zone = CreateZone();

        var restored = RoundTrip(zone);

        Assert.Equal(EvidenceAvailability.NotApplicable, restored.Figure.Availability);
        Assert.Empty(Validate(restored.Figure));
        Assert.Null(restored.Figure.NormalizedId.Value);
        Assert.True(restored.IsActive.Value);
        Assert.False(restored.IsConfirmed.Value);
        Assert.Equal(EvidenceAvailability.Present, restored.StrikeCount.Availability);
        Assert.Equal(0, restored.StrikeCount.Value);
        Assert.Equal(1, restored.OriginStageId.Value);
        Assert.Equal(zone.Anchors, restored.Anchors);
    }

    [Fact]
    public void StageTwoZone_PreservesKnownConceptWithUnresolvedIdAndInapplicableStrikes()
    {
        var figure = Figure("CHANNEL", "Canal descendente");
        var zone = CreateZone() with
        {
            Kind = Known(MapReferenceKind.FigureZone),
            Figure = figure,
            OriginStageId = Known(2),
            StrikeCount = Unavailable<int>(EvidenceAvailability.NotApplicable, "STAGE_TWO_NO_1234"),
            LastConfirmedPivotAtUtc = Unavailable<DateTimeOffset>(EvidenceAvailability.NotApplicable, "STAGE_TWO_NO_1234")
        };

        var restored = RoundTrip(zone);

        Assert.Empty(Validate(restored.Figure));
        Assert.Equal("Canal descendente", restored.Figure.ObservedLabel);
        Assert.Equal(EvidenceAvailability.Present, restored.Figure.Availability);
        Assert.Equal(EvidenceAvailability.Unknown, restored.Figure.NormalizedId.Availability);
        Assert.Null(restored.Figure.NormalizedId.Value);
        Assert.Equal(EvidenceAvailability.NotApplicable, restored.StrikeCount.Availability);
        Assert.Null(restored.StrikeCount.Value);
        Assert.Equal("STAGE_TWO_NO_1234", restored.StrikeCount.ReasonCode);
    }

    [Fact]
    public void SuppliedInclinedBounds_RetainReferenceTimeWithoutProjectingAnchors()
    {
        var knownAt = SetupAt.AddHours(2);
        var zone = CreateZone() with
        {
            InfluenceBounds = new ZoneInfluenceBoundsDto(
                Known(101m, ValidationSource.UserConfirmation, knownAt),
                Known(103m, ValidationSource.UserConfirmation, knownAt),
                Known(SetupAt, ValidationSource.UserConfirmation, knownAt)),
            Anchors =
            [
                new PriceTimeAnchorDto("pivot-1", Known(90m), Known(SetupAt.AddDays(-2))),
                new PriceTimeAnchorDto("pivot-2", Known(95m), Known(SetupAt.AddDays(-1)))
            ]
        };

        var restored = RoundTrip(zone);

        Assert.Equal(zone.InfluenceBounds, restored.InfluenceBounds);
        Assert.Equal(101m, restored.InfluenceBounds.LowerPrice.Value);
        Assert.Equal(103m, restored.InfluenceBounds.UpperPrice.Value);
        Assert.Equal(SetupAt, restored.InfluenceBounds.AtUtc.Value);
        Assert.Equal(knownAt, restored.InfluenceBounds.AtUtc.AvailableAtUtc);
        Assert.Equal(ValidationSource.UserConfirmation, restored.InfluenceBounds.LowerPrice.Source);
        Assert.Equal(zone.Anchors, restored.Anchors);
    }

    [Theory]
    [InlineData(EvaluationTimingMode.ClosedCandle)]
    [InlineData(EvaluationTimingMode.Intrabar)]
    [InlineData(EvaluationTimingMode.AnticipatedClose)]
    public void SetupContext_PreservesTimingModeAndDifferentTriggerFrame(EvaluationTimingMode timingMode)
    {
        var zone = CreateZone();
        var context = new EvaluationContextDto(10, 20, Known(2), Known(15), Known(SetupAt), Known(timingMode));

        var restored = RoundTrip(context);

        Assert.Equal(context, restored);
        Assert.Equal(timingMode, restored.TimingMode.Value);
        Assert.NotEqual(zone.OriginFrameId.Value, restored.TriggerFrameId.Value);
        Assert.NotEqual(zone.OriginStageId.Value, restored.StageId.Value);
    }

    [Fact]
    public void SetupContext_UnknownMomentAndModeRemainUnknownInsteadOfUsingReviewTime()
    {
        var context = new EvaluationContextDto(10, 20,
            Unavailable<int>(EvidenceAvailability.Unknown, "STAGE_NOT_IDENTIFIED"),
            Unavailable<int>(EvidenceAvailability.Unknown, "FRAME_NOT_IDENTIFIED"),
            Unavailable<DateTimeOffset>(EvidenceAvailability.Unknown, "SETUP_TIME_NOT_KNOWN"),
            Unavailable<EvaluationTimingMode>(EvidenceAvailability.Unknown, "TIMING_NOT_IDENTIFIED"));

        var restored = RoundTrip(context);

        Assert.Equal(context, restored);
        Assert.Null(restored.SetupAtUtc.Value);
        Assert.Null(restored.SetupAtUtc.AvailableAtUtc);
        Assert.Null(restored.TimingMode.Value);
        Assert.Equal(EvidenceAvailability.Unknown, restored.StageId.Availability);
    }

    [Fact]
    public void StructuralReferences_PreserveSeparateFiguresDirectionsFramesAndLocations()
    {
        var primary = Figure("CHANNEL", "Canal descendente");
        var references = new[]
        {
            new StructuralReferenceDto("channel", Known(MapReferenceKind.FigureZone), primary,
                Known(StructureDirection.Downward), Known(60), Known(2), Known(LocationType.Resistance),
                CreateZone().InfluenceBounds, [], Known(true)),
            new StructuralReferenceDto("stage-one-line", Known(MapReferenceKind.WorkingLine), NoFigure(),
                Known(StructureDirection.Upward), Known(240), Known(1), Known(LocationType.Support),
                CreateZone().InfluenceBounds, [], Known(true))
        };

        var restored = RoundTrip(references);

        Assert.Equal(2, restored.Length);
        Assert.Equal(primary, restored[0].Figure);
        Assert.Equal(StructureDirection.Downward, restored[0].Direction.Value);
        Assert.Equal(StructureDirection.Upward, restored[1].Direction.Value);
        Assert.NotEqual(restored[0].FrameId.Value, restored[1].FrameId.Value);
        Assert.Equal(LocationType.Resistance, restored[0].Location.Value);
        Assert.Equal(LocationType.Support, restored[1].Location.Value);
        Assert.Equal(EvidenceAvailability.NotApplicable, restored[1].Figure.Availability);
    }

    [Fact]
    public void CatalogNormalization_RetainsOriginalConceptProvenance()
    {
        var extractedAt = SetupAt.AddMinutes(1);
        var normalizedAt = SetupAt.AddMinutes(2);
        var extracted = Figure(null, "línea pivote") with
        {
            Source = ValidationSource.AiVision,
            AvailableAtUtc = extractedAt
        };
        var normalized = extracted with
        {
            NormalizedId = Known(123, ValidationSource.DeterministicRule, normalizedAt)
        };

        var restored = RoundTrip(normalized);

        Assert.Empty(Validate(restored));
        Assert.Equal(extracted.ObservedLabel, restored.ObservedLabel);
        Assert.Equal(ValidationSource.AiVision, restored.Source);
        Assert.Equal(extractedAt, restored.AvailableAtUtc);
        Assert.Equal(ValidationSource.DeterministicRule, restored.NormalizedId.Source);
        Assert.Equal(normalizedAt, restored.NormalizedId.AvailableAtUtc);
        Assert.Equal(EvidenceAvailability.Unknown, extracted.NormalizedId.Availability);
    }

    [Theory]
    [InlineData(EvidenceAvailability.Unknown)]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public void UnavailableCatalogConcept_PreservesItsDistinctState(EvidenceAvailability availability)
    {
        var reference = new CatalogReferenceDto(availability, null, null,
            Unavailable<int>(availability, "NO_CATALOG_REFERENCE"), null, null, null, "NO_CONCEPT");

        var restored = RoundTrip(reference);

        Assert.Empty(Validate(restored));
        Assert.Equal(reference, restored);
    }

    [Fact]
    public void PresentCatalogConcept_RequiresSomeKnownIdentification()
    {
        var reference = Figure(null, null);

        Assert.Contains(Validate(reference), error => error.MemberNames.Contains("ConceptCode"));
    }

    [Fact]
    public void CatalogIdAlone_CanRepresentAvailableHistoricalIdentification()
    {
        var reference = Figure(null, null) with { NormalizedId = Known(123) };

        Assert.Empty(Validate(reference));
        Assert.Null(reference.ConceptCode);
        Assert.Null(reference.ObservedLabel);
    }

    [Theory]
    [InlineData(EvidenceAvailability.Unknown)]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public void UnavailableCatalogConcept_RejectsClaimedIdentificationAndMissingReason(EvidenceAvailability availability)
    {
        var reference = new CatalogReferenceDto(availability, "CHANNEL", "Canal", Known(123), null, null, null, null);

        var errors = Validate(reference);

        Assert.Contains(errors, error => error.MemberNames.Contains("NormalizedId"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ReasonCode"));
    }

    [Fact]
    public void CatalogReference_ReportsInvalidNormalizedEvidenceWithItsFieldPath()
    {
        var reference = Figure("CHANNEL", "Canal") with
        {
            NormalizedId = new EvidenceValueDto<int>(EvidenceAvailability.Present, null, null, null, null, null, null)
        };

        Assert.Contains(Validate(reference), error => error.MemberNames.Contains("NormalizedId.Value"));
    }

    private static ZoneEvidenceDto CreateZone() => new(
        "working-line", Known(MapReferenceKind.WorkingLine), NoFigure(),
        new ZoneInfluenceBoundsDto(Known(99m), Known(101m), Known(SetupAt)),
        [new PriceTimeAnchorDto("origin-pivot", Known(100m), Known(SetupAt.AddDays(-1)))],
        Known(60), Known(1), Known(SetupAt.AddDays(-1)),
        Unavailable<DateTimeOffset>(EvidenceAvailability.Absent, "NO_INVALIDATION_OBSERVED"),
        Known(true), Known(false), Known(0),
        Unavailable<DateTimeOffset>(EvidenceAvailability.Unknown, "PIVOT_CONFIRMATION_TIME_NOT_KNOWN"));

    private static CatalogReferenceDto Figure(string? concept, string? label) => new(
        EvidenceAvailability.Present, concept, label,
        Unavailable<int>(EvidenceAvailability.Unknown, "CATALOG_NOT_NORMALIZED"),
        ValidationSource.UserInput, null, null, null);

    private static CatalogReferenceDto NoFigure() => new(
        EvidenceAvailability.NotApplicable, null, null,
        Unavailable<int>(EvidenceAvailability.NotApplicable, "NO_FIGURE_REQUIRED"),
        ValidationSource.UserInput, null, null, "NO_FIGURE_REQUIRED");

    private static EvidenceValueDto<T> Known<T>(T value, ValidationSource source = ValidationSource.UserInput,
        DateTimeOffset? availableAt = null) where T : struct =>
        new(EvidenceAvailability.Present, value, source, null, availableAt, null, null);

    private static EvidenceValueDto<T> Unavailable<T>(EvidenceAvailability availability, string reason) where T : struct =>
        new(availability, null, null, null, null, null, reason);

    private static T RoundTrip<T>(T value) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!;

    private static IReadOnlyList<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
