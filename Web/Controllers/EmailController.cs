using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IConfiguration _configuration;
        public EmailController(IEmployeeService employeeService, IConfiguration configuration)
        {
            _employeeService = employeeService;
            _configuration = configuration;
        }

        /// <summary>
        /// Metodo para confirmar email
        /// </summary>
        /// <param name="emailUser">email de la persona que se va a verificar</param>
        /// <param name="verificationCode">Codigo de verificación de la persona</param>
        /// <returns></returns>
        public async Task<IActionResult> ConfirmEmail(string emailUser, string verificationCode)
        {
            var result = await _employeeService.ConfirmEmployeeEmailAsync(emailUser, verificationCode);      

            ViewBag.name = result.Employee?.Name;
            ViewBag.verification = result.Verification;
            ViewBag.message = result.MessageError;
            ViewBag.emailSupport = _configuration["AppSettings:EmailSupport"];
            return View();
        }
    }
}
