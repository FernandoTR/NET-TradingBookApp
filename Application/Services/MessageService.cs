using Application.Interfaces;
using Microsoft.Extensions.Localization;
using Application.Resources;
using System.Reflection;
using System.Resources;

namespace Application.Services;

public class MessageService : IMessageService
{
    private static readonly ResourceManager ErrorMessageResourceManager =
         new ResourceManager("Application.Resources.ErrorMessage", Assembly.GetExecutingAssembly());

    private static readonly ResourceManager MessageResourceManager =
        new ResourceManager("Application.Resources.Message", Assembly.GetExecutingAssembly());

    private readonly ILogService _logService;

    public MessageService(ILogService logService) {  _logService = logService; }



    public string GetResourceError(string resourceName)
    {
        try
        {
            // Acceder a los recursos de error
            return ErrorMessageResourceManager.GetString(resourceName);
        }
        catch (Exception ex)
        {
            // Manejo de errores
            _logService.ErrorLog("ResourceService.GetResourceError", ex);
            return string.Empty;
        }
    }

    public string GetResourceMessage(string resourceName)
    {
        try
        {
            // Acceder a los recursos de mensaje
            return MessageResourceManager.GetString(resourceName);
        }
        catch (Exception ex)
        {
            // Manejo de errores
            _logService.ErrorLog("ResourceService.GetResourceMessage", ex);
            return string.Empty;
        }
    }
}
