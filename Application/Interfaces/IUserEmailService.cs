using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IUserEmailService
{
    Task<bool> SendEmailConfirmationAsync(string name, string mail, string codeSecurity, DateTime endDateValidation, UserTypes userType);
    Task<bool> SendEmailPasswordResetAsync(string name, string mail, string password);
    Task<bool> SendEmailNewUserAsync(string name, string mail, string password);
    Task<bool> SendEmailForgotPasswordAsync(string mail, string callbackUrl);
}
