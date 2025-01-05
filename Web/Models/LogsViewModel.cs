namespace Web.Models;

public class LogsViewModel
{
    public Int64 EventId { get; set; }
    public DateTime EventDate { get; set; }
    public string FormatedDate { get; set; }
    public string EventType { get; set; }
    public string Description { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }

}
