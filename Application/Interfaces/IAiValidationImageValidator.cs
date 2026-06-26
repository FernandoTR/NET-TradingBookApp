using Application.DTOs.AiValidation;
using Application.Models;

namespace Application.Interfaces;

public interface IAiValidationImageValidator
{
    Task<Result> ValidateAsync(IReadOnlyCollection<AiValidationImageInputDto> images, CancellationToken cancellationToken = default);
}
