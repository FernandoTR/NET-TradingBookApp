using Domain.Enums;

namespace Web.Models;

public sealed class AiValidationImageUploadViewModel
{
    public IFormFile File { get; set; } = null!;

    public string FrameCode { get; set; } = null!;

    public TradingImageRole ImageRole { get; set; }

    public int SortOrder { get; set; }

    public string? Comment { get; set; }
}
