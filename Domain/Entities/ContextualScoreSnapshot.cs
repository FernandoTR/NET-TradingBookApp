using Domain.Enums;

namespace Domain.Entities;

/// <summary>Persists one immutable contextual calculation and its reproducibility inputs.</summary>
public sealed class ContextualScoreSnapshot
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public ContextualScoreSourceKind SourceKind { get; set; }

    public int? OrderId { get; set; }

    public int? ValidationId { get; set; }

    public int? EvaluationRevision { get; set; }

    public string ScoreVersion { get; set; } = null!;

    public ContextualScoreState State { get; set; }

    public int? ContextRank { get; set; }

    public decimal? Value { get; set; }

    public GradeType? Grade { get; set; }

    public string DefinitionJson { get; set; } = null!;

    public string InputSnapshotJson { get; set; } = null!;

    public string ComponentsJson { get; set; } = null!;

    public DateTimeOffset CalculatedAtUtc { get; set; }
}
