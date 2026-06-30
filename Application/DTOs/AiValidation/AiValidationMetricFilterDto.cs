namespace Application.DTOs.AiValidation;

public class AiValidationMetricFilterDto
{
    public string? ProviderName { get; set; }

    public string? ModelName { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
}
