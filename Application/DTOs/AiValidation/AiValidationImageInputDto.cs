using Domain.Enums;

namespace Application.DTOs.AiValidation;

public sealed class AiValidationImageInputDto
{
    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    public string FrameCode { get; set; } = null!;

    public TradingImageRole ImageRole { get; set; }

    public int SortOrder { get; set; }

    public string? Comment { get; set; }

    public Stream Content { get; set; } = null!;
}
