

namespace Application.DTOs;

public class ParametersTBAnalyticsDto
{
    public int? CategoryId { get; set; }
    public int? AccountTypeId { get; set; }
    public int? InstrumentId { get; set; }
    public int? FrameId { get; set; }
    public string? SearchValue { get; set; }
    public string? OrderByColumn { get; set; }
    public string? SortColumnDir { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public int? Count { get; set; }


}
