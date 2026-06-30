using Application.DTOs.AiProviders;

namespace Application.Interfaces;

public interface IAiVisionClientFactory
{
    Task<AiVisionClientSelection> CreateActiveClientAsync(CancellationToken cancellationToken);
}
