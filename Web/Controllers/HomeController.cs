using Application.Interfaces;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using Web.Models;
using Web.Models.Enums;

namespace Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IIdentityService _identityService;
    private readonly ILogService _logService;

    public HomeController(IIdentityService identityService, ILogService logService)
    {
        _identityService = identityService;
        _logService = logService;
    }

    public IActionResult Index(NotificationType? notification, string message)
    {
        try
        {
            // Validamos si el usuario no ha iniciado sesión para redireccionarlo al login.
            var currentUser = _identityService.GetCurrentUserAsync();

            if (!User.Identity.IsAuthenticated || currentUser == null || !currentUser.IsValid)
            {
                return currentUser?.ResetFlag == true
                    ? RedirectToAction("ChangePassword", "Manage")
                    : RedirectToAction("SignIn", "Account");
            }

            // Si hay un mensaje y una notificación, lo agregamos al ViewData
            if (!string.IsNullOrEmpty(message) && notification != null)
            {
                ViewData[$"notifications.{notification}"] = message;
            }

            ViewBag.FullName = $"{currentUser.Name} {currentUser.LastName}";
        }
        catch (Exception ex)
        {
            _logService.ErrorLog($"Controller: Home, Action: {nameof(Index)}", ex);
            return RedirectToAction("Error", "Home");
        }


        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
