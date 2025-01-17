using Application.DTOs;

namespace Web.Models;

public class AnalyticsTriggerViewModel
{    
    public List<GetTBAnalyticsTriggerDto>? AnalyticsTriggerList { get; set; }
    public int TotalRecords { get; set; }
    public int TotalValidRecords { get; set; }

}
