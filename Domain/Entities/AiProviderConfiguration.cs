using System;

namespace Infrastructure;

public partial class AiProviderConfiguration
{
    public int Id { get; set; }

    public string ProviderName { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string? Endpoint { get; set; }

    public string ApiKeyEnvironmentVariable { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; } = 60;

    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeactivatedAt { get; set; }
}
