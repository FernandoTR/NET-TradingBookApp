using Application.DTOs;

namespace Web.Models;

public class AnalyticsSceneryViewModel
{
    public List<GetTBAnalyticsSceneryDto>? AnalyticsSceneryList { get; set; }
    public int TotalRecords { get; set; }
    public int TotalValidRecords { get; set; }
}
