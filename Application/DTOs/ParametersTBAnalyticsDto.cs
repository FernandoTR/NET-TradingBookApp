

namespace Application.DTOs;

public class ParametersTBAnalyticsDto : ParametersAnalyticsDto
{   
    public string? SearchValue { get; set; }
    public string? OrderByColumn { get; set; }
    public string? SortColumnDir { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public int? Count { get; set; }


}
