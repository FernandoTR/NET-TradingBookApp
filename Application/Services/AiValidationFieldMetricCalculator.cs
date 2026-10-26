using System.ComponentModel.DataAnnotations;
using Application.DTOs.AiValidation.V2;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

/// <summary>Calculates field-level metrics without imputing unknowns, merging populations or averaging percentages.</summary>
public sealed class AiValidationFieldMetricCalculator : IAiValidationFieldMetricCalculator
{
    /// <summary>The initial definition of field-level comparison and human-review metrics.</summary>
    public const string MetricsVersion = "validation-field-metrics-v2";

    /// <inheritdoc />
    public AiValidationFieldMetricMeasurementV2Dto Calculate(IReadOnlyList<AiValidationFieldMetricInputDto> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        var codes = new HashSet<string>(StringComparer.Ordinal);
        var applicable = new List<AiValidationFieldMetricInputDto>();
        var notApplicable = 0;
        foreach (var field in fields)
        {
            if (field is null || string.IsNullOrWhiteSpace(field.FieldCode) || !codes.Add(field.FieldCode))
                throw new ArgumentException("METRIC_FIELD_CODE_MISSING_OR_DUPLICATED", nameof(fields));
            if (field.AiObservation is null || field.HumanObservation is null ||
                !Validator.TryValidateObject(field.AiObservation, new ValidationContext(field.AiObservation), [], true) ||
                !Validator.TryValidateObject(field.HumanObservation, new ValidationContext(field.HumanObservation), [], true))
                throw new ArgumentException("METRIC_FIELD_OBSERVATION_INVALID", nameof(fields));

            if (!field.IsApplicable)
            {
                if (string.IsNullOrWhiteSpace(field.ApplicabilityReasonCode))
                    throw new ArgumentException("METRIC_INAPPLICABILITY_REASON_REQUIRED", nameof(fields));
                notApplicable++;
                continue;
            }

            if (field.AiObservation.Availability == EvidenceAvailability.NotApplicable ||
                field.HumanObservation.Availability == EvidenceAvailability.NotApplicable)
                throw new ArgumentException("METRIC_APPLICABLE_FIELD_CANNOT_HAVE_NOT_APPLICABLE_OBSERVATION", nameof(fields));

            if (!field.WasReviewed && field.HumanObservation.Availability != EvidenceAvailability.Unknown)
                throw new ArgumentException("METRIC_UNREVIEWED_FIELD_CANNOT_HAVE_HUMAN_VALUE", nameof(fields));

            applicable.Add(field);
        }

        var measurements = applicable.Select(Measure).ToArray();
        var coverageNumerator = applicable.Count(field => IsKnown(field.AiObservation.Availability));
        var concordanceCandidates = measurements.Where(metric => metric.Matches.HasValue).ToArray();
        var reviewedAiAssertions = measurements.Where(metric => metric.Field.WasReviewed &&
            IsKnown(metric.Field.AiObservation.Availability)).ToArray();
        var reviewedUnknownAi = measurements.Where(metric => metric.Field.WasReviewed &&
            metric.Field.AiObservation.Availability == EvidenceAvailability.Unknown).ToArray();
        var reviewedFields = measurements.Where(metric => metric.Field.WasReviewed).ToArray();

        return new AiValidationFieldMetricMeasurementV2Dto(
            MetricsVersion,
            Rate(coverageNumerator, applicable.Count),
            Rate(concordanceCandidates.Count(metric => metric.Matches == true), concordanceCandidates.Length),
            Rate(reviewedAiAssertions.Count(metric => metric.ChangeKind is AiValidationFieldChangeKind.Correction or AiValidationFieldChangeKind.Discarded),
                reviewedAiAssertions.Length),
            Rate(reviewedUnknownAi.Count(metric => metric.ChangeKind == AiValidationFieldChangeKind.ManualCompletion), reviewedUnknownAi.Length),
            Rate(reviewedFields.Count(metric => metric.ChangeKind == AiValidationFieldChangeKind.Discarded), reviewedFields.Length),
            measurements.Select(metric => metric.Dto).ToArray(), notApplicable,
            measurements.Count(metric => metric.ChangeKind == AiValidationFieldChangeKind.NoReview));
    }

    private static FieldMeasurement Measure(AiValidationFieldMetricInputDto field)
    {
        var ai = field.AiObservation;
        var human = field.HumanObservation;
        if (!field.WasReviewed)
            return new(field, Result(field, null, AiValidationFieldChangeKind.NoReview, "HUMAN_REVIEW_NOT_RECORDED"));

        if (ai.Availability == EvidenceAvailability.Unknown && IsKnown(human.Availability))
            return new(field, Result(field, null, AiValidationFieldChangeKind.ManualCompletion,
                field.ReviewReasonCode ?? "AI_UNKNOWN_COMPLETED_BY_USER"));

        if (IsKnown(ai.Availability) && human.Availability == EvidenceAvailability.Unknown)
            return new(field, Result(field, null, AiValidationFieldChangeKind.Discarded,
                field.ReviewReasonCode ?? human.ReasonCode ?? "AI_ASSERTION_DISCARDED_AS_UNDETERMINABLE"));

        if (ai.Availability == EvidenceAvailability.Unknown && human.Availability == EvidenceAvailability.Unknown)
            return new(field, Result(field, null, AiValidationFieldChangeKind.Discarded,
                field.ReviewReasonCode ?? human.ReasonCode ?? "FIELD_REVIEWED_BUT_UNDETERMINABLE"));

        var matches = ai.Availability == human.Availability &&
            (ai.Availability == EvidenceAvailability.Absent || string.Equals(ai.NormalizedValue, human.NormalizedValue, StringComparison.Ordinal));
        return new(field, Result(field, matches,
            matches ? AiValidationFieldChangeKind.NoChange : AiValidationFieldChangeKind.Correction,
            matches ? field.ReviewReasonCode : field.ReviewReasonCode ?? "AI_VALUE_CORRECTED_BY_USER"));
    }

    private static AiValidationFieldMetricDto Result(AiValidationFieldMetricInputDto field, bool? matches,
        AiValidationFieldChangeKind changeKind, string? reason) => new(
            field.FieldCode, field.AiObservation.Availability, field.HumanObservation.Availability,
            field.AiObservation.Source, field.HumanObservation.Source, matches, changeKind, reason,
            field.AiEvidenceReference, field.HumanEvidenceReference);

    private static bool IsKnown(EvidenceAvailability availability) =>
        availability is EvidenceAvailability.Present or EvidenceAvailability.Absent;

    private static RateMetricDto Rate(int numerator, int denominator) => new(
        numerator, denominator,
        denominator == 0 ? null : Math.Round(numerator * 100m / denominator, 2, MidpointRounding.AwayFromZero));

    private sealed record FieldMeasurement(AiValidationFieldMetricInputDto Field, AiValidationFieldMetricDto Dto)
    {
        public bool? Matches => Dto.Matches;
        public AiValidationFieldChangeKind ChangeKind => Dto.ChangeKind;
    }
}
