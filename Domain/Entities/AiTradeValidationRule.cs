namespace Infrastructure;

public partial class AiTradeValidationRule
{
    public int Id { get; set; }

    public int ValidationId { get; set; }

    public string RuleCode { get; set; } = null!;

    public string RuleName { get; set; } = null!;

    public string Result { get; set; } = null!;

    public decimal Weight { get; set; }

    public decimal ScoreObtained { get; set; }

    public string? Evidence { get; set; }

    public string Source { get; set; } = null!;

    public virtual AiTradeValidation Validation { get; set; } = null!;
}
