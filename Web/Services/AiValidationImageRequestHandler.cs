using Application.DTOs.AiValidation;
using Application.Interfaces;
using Application.Models;
using Web.Models;

namespace Web.Services;

public sealed class AiValidationImageRequestHandler
{
    private readonly IAiValidationImageValidator _validator;
    private readonly ILogService _logService;

    public AiValidationImageRequestHandler(IAiValidationImageValidator validator, ILogService logService)
    {
        _validator = validator;
        _logService = logService;
    }

    public async Task<Result> ValidateAndContinueAsync(
        IEnumerable<AiValidationImageUploadViewModel> uploads,
        Func<IReadOnlyList<AiValidationImageInputDto>, CancellationToken, Task<Result>> next,
        CancellationToken cancellationToken = default)
    {
        var uploadList = uploads.ToList();
        IReadOnlyList<AiValidationImageInputDto> images = [];

        try
        {
            images = await AiValidationImageInputMapper.MapAsync(uploadList, cancellationToken);
            var validationResult = await _validator.ValidateAsync(images, cancellationToken);

            if (!validationResult.Succeeded)
            {
                return validationResult;
            }

            return await next(images, cancellationToken);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(
                nameof(AiValidationImageRequestHandler),
                "Error tecnico al procesar imagenes de validacion IA.",
                BuildSafeLogDetails(uploadList, ex));

            return Result.Failure(["No fue posible procesar las imagenes. Intente nuevamente."]);
        }
        finally
        {
            await AiValidationImageInputMapper.DisposeAsync(images);
        }
    }

    private static string BuildSafeLogDetails(IReadOnlyCollection<AiValidationImageUploadViewModel> uploads, Exception exception)
    {
        var totalBytes = uploads.Sum(upload => upload.File?.Length ?? 0);
        var contentTypes = string.Join(",", uploads.Select(upload => upload.File?.ContentType).Where(contentType => !string.IsNullOrWhiteSpace(contentType)).Distinct());

        return $"UploadCount={uploads.Count}; TotalBytes={totalBytes}; ContentTypes={contentTypes}; ExceptionType={exception.GetType().Name}";
    }
}
