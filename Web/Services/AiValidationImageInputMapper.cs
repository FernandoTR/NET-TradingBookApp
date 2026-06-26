using Application.DTOs.AiValidation;
using Web.Models;

namespace Web.Services;

public static class AiValidationImageInputMapper
{
    public static async Task<IReadOnlyList<AiValidationImageInputDto>> MapAsync(
        IEnumerable<AiValidationImageUploadViewModel> images,
        CancellationToken cancellationToken = default)
    {
        var mappedImages = new List<AiValidationImageInputDto>();

        try
        {
            foreach (var image in images.OrderBy(image => image.SortOrder))
            {
                var content = new MemoryStream();
                await using (var uploadStream = image.File.OpenReadStream())
                {
                    await uploadStream.CopyToAsync(content, cancellationToken);
                }

                content.Position = 0;

                mappedImages.Add(new AiValidationImageInputDto
                {
                    OriginalFileName = image.File.FileName,
                    ContentType = image.File.ContentType,
                    FileSize = image.File.Length,
                    FrameCode = image.FrameCode,
                    ImageRole = image.ImageRole,
                    SortOrder = image.SortOrder,
                    Comment = image.Comment,
                    Content = content
                });
            }

            return mappedImages;
        }
        catch
        {
            await DisposeAsync(mappedImages);
            throw;
        }
    }

    public static async ValueTask DisposeAsync(IEnumerable<AiValidationImageInputDto> images)
    {
        foreach (var image in images)
        {
            await image.Content.DisposeAsync();
        }
    }
}
