using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class AiValidationFieldMetricCalculatorTests
{
    private static readonly DateTimeOffset OccurredAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");
    private static readonly DateTimeOffset ReviewedAt = OccurredAt.AddHours(1);
    private readonly AiValidationFieldMetricCalculator _calculator = new();

    [Fact]
    public void Calculate_DistinguishesNoChangeCorrectionCompletionDiscardNoReviewAndNonApplicability()
    {
        var fields = new[]
        {
            Field("boolean", Present("false", ValidationSource.AiVision), Present("false", ValidationSource.UserConfirmation), reviewed: true),
            Field("corrected", Present("0", ValidationSource.AiVision), Present("1", ValidationSource.UserConfirmation), reviewed: true,
                reviewReason: "USER_CORRECTED_VALUE"),
            Field("known-absence", Absent("REFERENCE_ABSENT", ValidationSource.AiVision), Absent("REFERENCE_ABSENT", ValidationSource.UserConfirmation), reviewed: true),
            Field("completed", Unknown("NOT_EXTRACTED"), Present("false", ValidationSource.UserConfirmation), reviewed: true),
            Field("discarded-claim", Present("LP", ValidationSource.AiVision), Unknown("NOT_DETERMINABLE", ValidationSource.UserConfirmation),
                reviewed: true, reviewReason: "VISUAL_EVIDENCE_UNDETERMINABLE"),
            Field("discarded-unknown", Unknown("NOT_EXTRACTED"), Unknown("NOT_DETERMINABLE", ValidationSource.UserConfirmation), reviewed: true),
            Field("no-review", Unknown("NOT_EXTRACTED"), Unknown("HUMAN_REVIEW_NOT_RECORDED"), reviewed: false),
            Field("stage-two-trigger", NotApplicable("NO_TRIGGER_FIELD"), NotApplicable("NO_TRIGGER_FIELD"), reviewed: false,
                applicable: false, applicabilityReason: "TRIGGER_NOT_APPLICABLE_TO_STAGE")
        };

        var summary = _calculator.Calculate(fields);

        Assert.Equal(AiValidationFieldMetricCalculator.MetricsVersion, summary.MetricsVersion);
        Assert.Equal(1, summary.NotApplicableFieldCount);
        Assert.Equal(1, summary.NoReviewCount);
        Assert.Equal(7, summary.Fields.Count);
        AssertMetric(summary.Fields, "boolean", AiValidationFieldChangeKind.NoChange, true);
        AssertMetric(summary.Fields, "corrected", AiValidationFieldChangeKind.Correction, false);
        AssertMetric(summary.Fields, "known-absence", AiValidationFieldChangeKind.NoChange, true);
        AssertMetric(summary.Fields, "completed", AiValidationFieldChangeKind.ManualCompletion, null);
        AssertMetric(summary.Fields, "discarded-claim", AiValidationFieldChangeKind.Discarded, null);
        AssertMetric(summary.Fields, "discarded-unknown", AiValidationFieldChangeKind.Discarded, null);
        AssertMetric(summary.Fields, "no-review", AiValidationFieldChangeKind.NoReview, null);
        Assert.DoesNotContain(summary.Fields, metric => metric.FieldCode == "stage-two-trigger");

        AssertRate(summary.Coverage, 4, 7, 57.14m);
        AssertRate(summary.Concordance, 2, 3, 66.67m);
        AssertRate(summary.HumanCorrections, 2, 4, 50m);
        AssertRate(summary.ManualCompletions, 1, 2, 50m);
        AssertRate(summary.Discards, 2, 6, 33.33m);
        Assert.Equal("VISUAL_EVIDENCE_UNDETERMINABLE", summary.Fields.Single(field => field.FieldCode == "discarded-claim").ReasonCode);
    }

    [Fact]
    public void FalseAndZero_AreComparableKnownValuesAndNotAbsence()
    {
        var summary = _calculator.Calculate(
        [
            Field("false", Present("false", ValidationSource.AiVision), Present("false", ValidationSource.UserConfirmation), reviewed: true),
            Field("zero", Present("0", ValidationSource.AiVision), Present("0", ValidationSource.UserConfirmation), reviewed: true),
            Field("absence", Absent("CONFIRMED_ABSENT", ValidationSource.AiVision), Absent("CONFIRMED_ABSENT", ValidationSource.UserConfirmation), reviewed: true)
        ]);

        AssertRate(summary.Coverage, 3, 3, 100m);
        AssertRate(summary.Concordance, 3, 3, 100m);
        AssertRate(summary.HumanCorrections, 0, 3, 0m);
        AssertRate(summary.ManualCompletions, 0, 0, null);
        Assert.Equal(EvidenceAvailability.Present, summary.Fields.Single(field => field.FieldCode == "false").AiAvailability);
        Assert.Equal(EvidenceAvailability.Absent, summary.Fields.Single(field => field.FieldCode == "absence").AiAvailability);
    }

    [Fact]
    public void NoComparableData_ReturnsNullPercentagesAndKeepsZeroDenominators()
    {
        var summary = _calculator.Calculate([]);

        AssertRate(summary.Coverage, 0, 0, null);
        AssertRate(summary.Concordance, 0, 0, null);
        AssertRate(summary.HumanCorrections, 0, 0, null);
        AssertRate(summary.ManualCompletions, 0, 0, null);
        AssertRate(summary.Discards, 0, 0, null);
        Assert.Empty(summary.Fields);
        Assert.Equal(0, summary.NotApplicableFieldCount);
        Assert.Equal(0, summary.NoReviewCount);
    }

    [Fact]
    public void AiUnknownAndHumanUnknown_AreNotComparableOrCountedAsACompletion()
    {
        var summary = _calculator.Calculate(
        [
            Field("not-determined", Unknown("AI_NOT_DETERMINABLE"), Unknown("HUMAN_NOT_DETERMINABLE", ValidationSource.UserConfirmation),
                reviewed: true, reviewReason: "NO_VALUE_CAN_BE_DETERMINED")
        ]);

        AssertRate(summary.Coverage, 0, 1, 0m);
        AssertRate(summary.Concordance, 0, 0, null);
        AssertRate(summary.HumanCorrections, 0, 0, null);
        AssertRate(summary.ManualCompletions, 0, 1, 0m);
        AssertRate(summary.Discards, 1, 1, 100m);
        Assert.Null(summary.Fields[0].Matches);
    }

    [Fact]
    public void FieldCodeAndEvidenceReferences_AreRetainedWithoutStoringObservedScalarsInTheMetric()
    {
        var metricInput = Field("figure", Present("LP", ValidationSource.AiVision),
            Present("LP", ValidationSource.UserConfirmation), reviewed: true) with
        {
            AiEvidenceReference = "evidence-1.figure.ai",
            HumanEvidenceReference = "evidence-1.figure.confirmation"
        };

        var field = Assert.Single(_calculator.Calculate([metricInput]).Fields);

        Assert.Equal("figure", field.FieldCode);
        Assert.Equal("evidence-1.figure.ai", field.AiEvidenceReference);
        Assert.Equal("evidence-1.figure.confirmation", field.HumanEvidenceReference);
        Assert.DoesNotContain("LP", System.Text.Json.JsonSerializer.Serialize(field));
    }

    [Fact]
    public void UnreviewedField_DoesNotContributeToConcordanceOrCorrectionDenominators()
    {
        var summary = _calculator.Calculate(
        [
            Field("reviewed", Present("LP", ValidationSource.AiVision), Present("CD", ValidationSource.UserConfirmation), reviewed: true),
            Field("unreviewed", Present("LP", ValidationSource.AiVision), Unknown("NOT_REVIEWED"), reviewed: false)
        ]);

        AssertRate(summary.Coverage, 2, 2, 100m);
        AssertRate(summary.Concordance, 0, 1, 0m);
        AssertRate(summary.HumanCorrections, 1, 1, 100m);
        Assert.Equal(1, summary.NoReviewCount);
        Assert.Equal(AiValidationFieldChangeKind.NoReview, summary.Fields.Single(field => field.FieldCode == "unreviewed").ChangeKind);
    }

    [Fact]
    public void NonApplicableField_IsExcludedOnlyWhenItHasAnExplicitReason()
    {
        var notApplicable = Field("trigger", NotApplicable("STAGE_TWO_NO_STAGE_ONE_TRIGGER"),
            NotApplicable("STAGE_TWO_NO_STAGE_ONE_TRIGGER"), reviewed: false, applicable: false,
            applicabilityReason: "SETUP_DOES_NOT_REQUIRE_STAGE_ONE_TRIGGER");

        var summary = _calculator.Calculate([notApplicable]);

        Assert.Equal(1, summary.NotApplicableFieldCount);
        Assert.Empty(summary.Fields);
        AssertRate(summary.Coverage, 0, 0, null);
    }

    [Fact]
    public void DuplicateOrMissingFieldCodes_AreRejectedRatherThanDoubleCounted()
    {
        var first = Field("duplicate", Unknown("NOT_EXTRACTED"), Unknown("NOT_REVIEWED"), reviewed: false);

        Assert.Throws<ArgumentException>(() => _calculator.Calculate([first, first]));
        Assert.Throws<ArgumentException>(() => _calculator.Calculate([first with { FieldCode = " " }]));
    }

    [Fact]
    public void UnreviewedHumanValue_AndNotApplicableObservationOnApplicableField_AreRejected()
    {
        var unreviewedValue = Field("field", Present("A", ValidationSource.AiVision), Present("B", ValidationSource.UserConfirmation), reviewed: false);
        var mismatchedApplicability = Field("field", NotApplicable("NA"), Unknown("UNREVIEWED"), reviewed: false);

        Assert.Throws<ArgumentException>(() => _calculator.Calculate([unreviewedValue]));
        Assert.Throws<ArgumentException>(() => _calculator.Calculate([mismatchedApplicability]));
    }

    [Fact]
    public void InvalidObservationAvailabilityContract_IsRejected()
    {
        var invalid = Field("field", new(EvidenceAvailability.Present, null, ValidationSource.AiVision, OccurredAt, ReviewedAt, "MISSING_VALUE"),
            Unknown("NOT_REVIEWED"), reviewed: false);

        Assert.Throws<ArgumentException>(() => _calculator.Calculate([invalid]));
    }

    [Fact]
    public void Calculation_DoesNotMutateAiOrHumanObservations()
    {
        var input = Field("field", Present("false", ValidationSource.AiVision), Present("true", ValidationSource.UserConfirmation), reviewed: true);
        var aiBefore = input.AiObservation;
        var humanBefore = input.HumanObservation;

        _calculator.Calculate([input]);

        Assert.Equal(aiBefore, input.AiObservation);
        Assert.Equal(humanBefore, input.HumanObservation);
    }

    [Fact]
    public void ApplicationRegistration_ResolvesTheVersionedFieldMetricCalculator()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.AddApplicationServices();
        using var provider = builder.Services.BuildServiceProvider();

        var calculator = provider.GetRequiredService<IAiValidationFieldMetricCalculator>();

        Assert.Equal(AiValidationFieldMetricCalculator.MetricsVersion, calculator.Calculate([]).MetricsVersion);
    }

    private static AiValidationFieldMetricInputDto Field(string code, AiValidationFieldObservationDto ai,
        AiValidationFieldObservationDto human, bool reviewed, bool applicable = true, string? applicabilityReason = null,
        string? reviewReason = null) => new(code, applicable, ai, human, reviewed, applicabilityReason, reviewReason);

    private static AiValidationFieldObservationDto Present(string value, ValidationSource source) =>
        new(EvidenceAvailability.Present, value, source, OccurredAt, source == ValidationSource.UserConfirmation ? ReviewedAt : OccurredAt, null);

    private static AiValidationFieldObservationDto Absent(string reason, ValidationSource source) =>
        new(EvidenceAvailability.Absent, null, source, OccurredAt, source == ValidationSource.UserConfirmation ? ReviewedAt : OccurredAt, reason);

    private static AiValidationFieldObservationDto Unknown(string reason, ValidationSource? source = null) =>
        new(EvidenceAvailability.Unknown, null, source, null, source == ValidationSource.UserConfirmation ? ReviewedAt : null, reason);

    private static AiValidationFieldObservationDto NotApplicable(string reason) =>
        new(EvidenceAvailability.NotApplicable, null, ValidationSource.DeterministicRule, null, OccurredAt, reason);

    private static void AssertMetric(IReadOnlyList<AiValidationFieldMetricDto> fields, string code,
        AiValidationFieldChangeKind change, bool? matches)
    {
        var metric = fields.Single(field => field.FieldCode == code);
        Assert.Equal(change, metric.ChangeKind);
        Assert.Equal(matches, metric.Matches);
    }

    private static void AssertRate(RateMetricDto rate, int numerator, int denominator, decimal? percentage)
    {
        Assert.Equal(numerator, rate.Numerator);
        Assert.Equal(denominator, rate.Denominator);
        Assert.Equal(percentage, rate.Percentage);
    }
}
