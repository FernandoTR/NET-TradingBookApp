using Web.Models.Enums;

namespace Web.Models;

public class ResultBackViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public NotificationType notificationType { get; set; }
    public int Code { get; set; }
}
