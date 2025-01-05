namespace Application.Interfaces;

public interface ILogService
{
    void ErrorLog(string methodName, Exception exception);
    void ErrorLog(string methodName, string message, string details);
    void ActivityLog(string userId, string eventType, string description);
}
