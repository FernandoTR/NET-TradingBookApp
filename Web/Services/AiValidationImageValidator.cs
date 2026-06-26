using Application.Common;
using Application.DTOs.AiValidation;
using Application.Interfaces;
using Application.Models;
using Microsoft.Extensions.Options;

namespace Web.Services;

public sealed class AiValidationImageValidator : IAiValidationImageValidator
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly AiTradeValidationOptions _options;

    public AiValidationImageValidator(IOptions<AiTradeValidationOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Result> ValidateAsync(IReadOnlyCollection<AiValidationImageInputDto> images, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (images.Count == 0)
        {
            errors.Add("Debe cargar al menos una imagen.");
        }

        if (images.Count > _options.MaxImagesPerValidation)
        {
            errors.Add($"Solo se permiten hasta {_options.MaxImagesPerValidation} imagenes por validacion.");
        }

        var maxImageSizeBytes = MegabytesToBytes(_options.MaxImageSizeMb);
        var maxTotalUploadBytes = MegabytesToBytes(_options.MaxTotalUploadMb);
        var totalSize = images.Sum(image => image.FileSize);

        if (totalSize > maxTotalUploadBytes)
        {
            errors.Add($"El tamanio total de las imagenes no debe exceder {_options.MaxTotalUploadMb} MB.");
        }

        var allowedContentTypes = _options.AllowedContentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var index = 0;

        foreach (var image in images)
        {
            index++;

            if (image.FileSize <= 0)
            {
                errors.Add($"La imagen {index} esta vacia.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(image.FrameCode))
            {
                errors.Add($"La imagen {index} requiere temporalidad.");
            }

            if (!Enum.IsDefined(image.ImageRole))
            {
                errors.Add($"La imagen {index} requiere un rol valido.");
            }

            if (image.SortOrder <= 0)
            {
                errors.Add($"La imagen {index} requiere un orden mayor a cero.");
            }

            if (image.FileSize > maxImageSizeBytes)
            {
                errors.Add($"La imagen {index} excede el tamanio maximo de {_options.MaxImageSizeMb} MB.");
            }

            if (!allowedContentTypes.Contains(image.ContentType))
            {
                errors.Add($"La imagen {index} tiene un tipo MIME no permitido.");
            }

            var extension = Path.GetExtension(image.OriginalFileName);
            if (string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(image.ContentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"La imagen {index} es SVG y no esta permitida.");
                continue;
            }

            if (!AllowedExtensions.Contains(extension))
            {
                errors.Add($"La imagen {index} tiene una extension no permitida.");
            }

            if (!await HasAllowedSignatureAsync(image.Content, cancellationToken))
            {
                errors.Add($"La imagen {index} no coincide con una firma valida JPEG, PNG o WebP.");
            }
        }

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }

    private static long MegabytesToBytes(int megabytes)
    {
        return megabytes * 1024L * 1024L;
    }

    private static async Task<bool> HasAllowedSignatureAsync(Stream content, CancellationToken cancellationToken)
    {
        if (!content.CanRead)
        {
            return false;
        }

        var originalPosition = content.CanSeek ? content.Position : (long?)null;

        try
        {
            if (content.CanSeek)
            {
                content.Position = 0;
            }

            var buffer = new byte[512];
            var bytesRead = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            return HasJpegSignature(buffer, bytesRead) ||
                   HasPngSignature(buffer, bytesRead) ||
                   HasWebpSignature(buffer, bytesRead);
        }
        finally
        {
            if (originalPosition.HasValue)
            {
                content.Position = originalPosition.Value;
            }
        }
    }

    private static bool HasJpegSignature(byte[] buffer, int bytesRead)
    {
        return bytesRead >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
    }

    private static bool HasPngSignature(byte[] buffer, int bytesRead)
    {
        return bytesRead >= 8 &&
               buffer[0] == 0x89 &&
               buffer[1] == 0x50 &&
               buffer[2] == 0x4E &&
               buffer[3] == 0x47 &&
               buffer[4] == 0x0D &&
               buffer[5] == 0x0A &&
               buffer[6] == 0x1A &&
               buffer[7] == 0x0A;
    }

    private static bool HasWebpSignature(byte[] buffer, int bytesRead)
    {
        return bytesRead >= 12 &&
               buffer[0] == 0x52 &&
               buffer[1] == 0x49 &&
               buffer[2] == 0x46 &&
               buffer[3] == 0x46 &&
               buffer[8] == 0x57 &&
               buffer[9] == 0x45 &&
               buffer[10] == 0x42 &&
               buffer[11] == 0x50;
    }
}
