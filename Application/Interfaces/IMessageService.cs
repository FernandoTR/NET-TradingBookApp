
namespace Application.Interfaces;

public interface IMessageService
{
    string GetResourceError(string resourceName);
    string GetResourceMessage(string resourceName);
}
