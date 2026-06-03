namespace Application.DTOs;

public class ParametersTBAnalyticsConvergenceDto : ParametersTBAnalyticsDto
{
    public int? TriggerId { get; set; }
    public int? SceneryId { get; set; }
    public int? FigureId { get; set; }

    public bool TriggerActive { get; set; }
    public bool SceneryActive { get; set; }
    public bool DirectionActive { get; set; }
    public bool FrameActive { get; set; }
    public bool FigureActive { get; set; }

    public int? MinTrades { get; set; }
}
