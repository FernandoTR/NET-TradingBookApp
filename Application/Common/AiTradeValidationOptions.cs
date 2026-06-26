namespace Application.Common;

public sealed class AiTradeValidationOptions
{
    public const string SectionName = "AiTradeValidation";

    public int MaxImagesPerValidation { get; set; } = 4;

    public int MaxImageSizeMb { get; set; } = 8;

    public int MaxTotalUploadMb { get; set; } = 32;

    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}
