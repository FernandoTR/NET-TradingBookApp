using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.DTOs.AiValidation.V2;
using Domain.Enums;

namespace Application.Tests;

public class EvidenceContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void PresentFalseAndZero_RemainKnownAcrossJsonRoundTrip()
    {
        var falseObservation = Evidence<bool>(EvidenceAvailability.Present, false);
        var zeroObservation = Evidence<decimal>(EvidenceAvailability.Present, 0m);

        var restoredFalse = RoundTrip(falseObservation);
        var restoredZero = RoundTrip(zeroObservation);

        Assert.Empty(Validate(restoredFalse));
        Assert.Empty(Validate(restoredZero));
        Assert.Equal(falseObservation, restoredFalse);
        Assert.Equal(zeroObservation, restoredZero);
        Assert.True(restoredFalse.Value.HasValue);
        Assert.False(restoredFalse.Value.Value);
        Assert.Equal(0m, restoredZero.Value);
    }

    [Theory]
    [InlineData(EvidenceAvailability.Unknown)]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public void UnavailableEvidence_RetainsStateAndReasonWithoutInventingMetadata(EvidenceAvailability availability)
    {
        var observation = Evidence<int>(availability, null, "TEST_REASON");

        var restored = RoundTrip(observation);

        Assert.Empty(Validate(restored));
        Assert.Equal(observation, restored);
        Assert.Null(restored.Value);
        Assert.Null(restored.Source);
        Assert.Null(restored.OccurredAtUtc);
        Assert.Null(restored.AvailableAtUtc);
        Assert.Null(restored.VisualConfidence);
    }

    [Fact]
    public void PresentEvidence_RequiresAValue()
    {
        var errors = Validate(Evidence<int>(EvidenceAvailability.Present, null));

        Assert.Contains(errors, error => error.MemberNames.Contains("Value"));
    }

    [Theory]
    [InlineData(EvidenceAvailability.Unknown)]
    [InlineData(EvidenceAvailability.Absent)]
    [InlineData(EvidenceAvailability.NotApplicable)]
    public void UnavailableEvidence_RejectsAValueEvenWhenZero(EvidenceAvailability availability)
    {
        var errors = Validate(Evidence<int>(availability, 0, "TEST_REASON"));

        Assert.Contains(errors, error => error.MemberNames.Contains("Value"));
    }

    [Theory]
    [InlineData(EvidenceAvailability.Unknown, null)]
    [InlineData(EvidenceAvailability.Unknown, "")]
    [InlineData(EvidenceAvailability.Absent, " ")]
    [InlineData(EvidenceAvailability.NotApplicable, null)]
    public void UnavailableEvidence_RequiresAnIdentifiableReason(EvidenceAvailability availability, string? reason)
    {
        var errors = Validate(Evidence<int>(availability, null, reason));

        Assert.Contains(errors, error => error.MemberNames.Contains("ReasonCode"));
    }

    [Fact]
    public void Evidence_RejectsUndefinedAvailability()
    {
        var errors = Validate(Evidence<int>((EvidenceAvailability)99, null, "TEST_REASON"));

        Assert.Contains(errors, error => error.MemberNames.Contains("Availability"));
    }

    [Fact]
    public void HumanConfirmation_RemainsSeparateFromExtractionAndDoesNotBackdateAvailability()
    {
        var occurredAt = DateTimeOffset.Parse("2026-10-23T10:00:00Z");
        var extractedAt = occurredAt.AddMinutes(1);
        var reviewedAt = occurredAt.AddHours(2);
        var extraction = new EvidenceValueDto<bool>(
            EvidenceAvailability.Present, false, ValidationSource.AiVision,
            occurredAt, extractedAt, 0.8m, null);
        var confirmation = extraction with
        {
            Value = true,
            Source = ValidationSource.UserConfirmation,
            AvailableAtUtc = reviewedAt,
            VisualConfidence = null
        };

        var restored = RoundTrip(confirmation);

        Assert.Empty(Validate(restored));
        Assert.Equal(confirmation, restored);
        Assert.Equal(occurredAt, restored.OccurredAtUtc);
        Assert.Equal(reviewedAt, restored.AvailableAtUtc);
        Assert.False(extraction.Value);
        Assert.Equal(extractedAt, extraction.AvailableAtUtc);
        Assert.Equal(ValidationSource.AiVision, extraction.Source);
    }

    [Fact]
    public void Evidence_RoundTripsTemporalAndEnumValues()
    {
        var temporal = Evidence<DateTimeOffset>(
            EvidenceAvailability.Present, DateTimeOffset.Parse("2026-10-23T10:00:00Z"));
        var enumValue = Evidence<SetupConditionState>(EvidenceAvailability.Present, SetupConditionState.Pending);

        Assert.Equal(temporal, RoundTrip(temporal));
        Assert.Equal(enumValue, RoundTrip(enumValue));
        Assert.Empty(Validate(temporal));
        Assert.Empty(Validate(enumValue));
    }

    [Fact]
    public void Conflict_DefaultsToUnresolvedAndPreservesOriginalReferencesAfterResolution()
    {
        var conflict = new EvidenceConflictDto("Trigger.IsClosed", ["extraction.isClosed", "user.isClosed"]);
        var resolved = conflict with
        {
            IsResolved = true,
            ResolutionObservationReference = "confirmation.isClosed",
            ResolutionReason = "The reviewer confirmed the candle closing time."
        };

        var restored = RoundTrip(resolved);

        Assert.Empty(Validate(conflict));
        Assert.False(conflict.IsResolved);
        Assert.Null(conflict.ResolutionObservationReference);
        Assert.Empty(Validate(restored));
        Assert.True(restored.IsResolved);
        Assert.Equal(conflict.ObservationReferences, restored.ObservationReferences);
        Assert.Equal(resolved.ResolutionObservationReference, restored.ResolutionObservationReference);
        Assert.Equal(resolved.ResolutionReason, restored.ResolutionReason);
    }

    [Fact]
    public void ResolvedConflict_RequiresSeparateConfirmationAndRationale()
    {
        var conflict = new EvidenceConflictDto("Trigger.IsClosed", ["ai", "user"], true);

        var missingResolution = Validate(conflict);
        var reusedOriginal = Validate(conflict with { ResolutionObservationReference = "ai", ResolutionReason = "Reviewed" });

        Assert.Contains(missingResolution, error => error.MemberNames.Contains("ResolutionObservationReference"));
        Assert.Contains(missingResolution, error => error.MemberNames.Contains("ResolutionReason"));
        Assert.Contains(reusedOriginal, error => error.MemberNames.Contains("ResolutionObservationReference"));
    }

    [Fact]
    public void UnresolvedConflict_RejectsResolutionData()
    {
        var conflict = new EvidenceConflictDto("Trigger.IsClosed", ["ai", "user"], ResolutionReason: "Reviewed");

        Assert.Contains(Validate(conflict), error => error.MemberNames.Contains("IsResolved"));
    }

    [Theory]
    [InlineData()]
    [InlineData("ai")]
    [InlineData("ai", "ai")]
    [InlineData("ai", " ")]
    public void Conflict_RequiresDistinctIdentifiableObservations(params string[] references)
    {
        var conflict = new EvidenceConflictDto("Trigger.IsClosed", references);

        Assert.Contains(Validate(conflict), error => error.MemberNames.Contains("ObservationReferences"));
    }

    [Fact]
    public void Conflict_RequiresAnAffectedField()
    {
        var conflict = new EvidenceConflictDto(" ", ["ai", "user"]);

        Assert.Contains(Validate(conflict), error => error.MemberNames.Contains("FieldCode"));
    }

    [Fact]
    public void DefaultStates_DoNotClaimEvidenceValidityOrScoreCompletion()
    {
        Assert.Equal(EvidenceAvailability.Unknown, default);
        Assert.Equal(SetupConditionState.Pending, default);
        Assert.Equal(SetupVerdict.Pending, default);
        Assert.Equal(ContextualScoreState.NotEvaluable, default);
    }

    private static EvidenceValueDto<T> Evidence<T>(EvidenceAvailability availability, T? value, string? reason = null)
        where T : struct => new(availability, value, null, null, null, null, reason);

    private static T RoundTrip<T>(T value) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!;

    private static IReadOnlyList<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
