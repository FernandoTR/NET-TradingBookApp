namespace Application.Common;

public sealed class AiTradeValidationOptions
{
    public const string SectionName = "AiTradeValidation";

    public int MaxImagesPerValidation { get; set; } = 2;

    public int MaxImageSizeMb { get; set; } = 4;

    public int MaxTotalUploadMb { get; set; } = 8;

    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];

    public decimal MinimumRiskRewardRatio { get; set; } = 1m;

    public int MinHistoricalEvidenceTrades { get; set; } = 10;
}
