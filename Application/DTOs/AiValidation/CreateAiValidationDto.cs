namespace Application.DTOs.AiValidation;

public class CreateAiValidationDto
{
    public string UserId { get; set; } = null!;

    public int InstrumentId { get; set; }

    public int DirectionId { get; set; }

    public decimal EntryPrice { get; set; }

    public decimal StopLoss { get; set; }

    public decimal TakeProfit { get; set; }

    public string? UserComment { get; set; }
}
