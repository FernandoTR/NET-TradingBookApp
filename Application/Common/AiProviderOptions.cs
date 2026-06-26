namespace Application.Common;

public sealed class AiProviderOptions
{
    public const string SectionName = "Ai";

    public string ActiveProvider { get; set; } = null!;

    public string ActiveModel { get; set; } = null!;

    public Dictionary<string, AiProviderDefinition> Providers { get; set; } = new();
}

public sealed class AiProviderDefinition
{
    public string Model { get; set; } = null!;

    public string? Endpoint { get; set; }

    public string ApiKeyEnvironmentVariable { get; set; } = null!;

    public bool SupportsVision { get; set; }

    public int TimeoutSeconds { get; set; } = 60;
}
