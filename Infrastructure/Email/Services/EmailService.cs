using Application.DTOs;
using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Collections.Concurrent;

namespace Infrastructure.Email.Services;

/// <summary>
/// Esta clase maneja la lógica general del envío de correos (SMTP, configuración del cliente, etc.)
/// </summary>
public class EmailService: IEmailSender
{
    private readonly ConcurrentQueue<SmtpClient> _clients = new ConcurrentQueue<SmtpClient>();
    private readonly SmtpClient _smtpClient;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public EmailService(IConfiguration configuration, ILogService logService)
    {
        _smtpServer = configuration["EmailSettings:SmtpServer"];
        _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
        _smtpUser = configuration["EmailSettings:SmtpUser"];
        _smtpPassword = configuration["EmailSettings:SmtpPassword"];
        _configuration = configuration;
        _logService = logService;
    }

    /// <summary>
    /// Envía un correo electrónico de forma asíncrona.
    /// </summary>
    /// <param name="email">Correo(s) destinatario(s). Para múltiples correos, separarlos con ";".</param>
    /// <param name="subject">Asunto del correo.</param>
    /// <param name="htmlMessage">Mensaje en formato HTML.</param>
    /// <returns>Una tarea representando la operación asíncrona.</returns>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("WebAppBase", _smtpUser));
            
            // Manejo de múltiples destinatarios separados por ';'
            var recipients = email.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(e => e.Trim())
                                   .Distinct();

            foreach (var recipient in recipients)
            {
                message.To.Add(new MailboxAddress(null, recipient));
            }

            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();         



            using var _smtpClient = new SmtpClient();
            await _smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await _smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
            await _smtpClient.SendAsync(message);
            await _smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailAsync), ex);
        }
        finally
        {
            _clients.Enqueue(_smtpClient);
        }

    }

    /// <summary>
    /// Envía un correo electrónico de forma asíncrona.
    /// </summary>
    /// <param name="messageDto"></param>
    /// <returns>Una tarea representando la operación asíncrona.</returns>
    public async Task SendEmailAsync(EmailMessageDto messageDto)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("WebAppBase", _smtpUser));

            // Manejo de múltiples destinatarios separados por ';'
            var recipients = messageDto.Destination.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(e => e.Trim())
                                                   .Distinct();

            foreach (var recipient in recipients)
            {
                message.To.Add(new MailboxAddress(null, recipient));
            }

            message.Subject = messageDto.Subject;
            message.Body = new BodyBuilder { HtmlBody = messageDto.Body }.ToMessageBody();



            using var _smtpClient = new SmtpClient();
            await _smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await _smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
            await _smtpClient.SendAsync(message);
            await _smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailAsync), ex);
        }
        finally
        {
            _clients.Enqueue(_smtpClient);
        }

    }

    /// <summary>
    /// Envia correos con la lista de archivos adjuntos guardados en el servidor 
    /// </summary>
    /// <param name="messageDto"></param>
    /// <param name="files"></param>
    /// <param name="readReceipt">Indica si se solicitará una confirmación de lectura</param>
    /// <returns>Una tarea representando la operación asíncrona.</returns>
    public async Task SendEmailAsync(EmailMessageDto messageDto, List<string> files, bool? readReceipt = false)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("WebAppBase", _smtpUser));

            // Manejo de múltiples destinatarios separados por ';'
            var recipients = messageDto.Destination.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(e => e.Trim())
                                                   .Distinct();

            foreach (var recipient in recipients)
            {
                message.To.Add(new MailboxAddress(null, recipient));
            }

            // Crear el cuerpo del correo
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = messageDto.Body, // Cuerpo del correo en HTML
            };

            

            if (Convert.ToBoolean(readReceipt))
            {
                // Agregar encabezado de confirmación de lectura. Nota: en Gmail sólo funciona para cuentas empresariales.
                message.Headers.Add("Disposition-Notification-To", _smtpUser);
            }

            // Agregar adjuntos
            if (files != null)
            {
                foreach (string file in files)
                {
                    if (File.Exists(@file))
                        bodyBuilder.Attachments.Add(@file);
                }
            }

            message.Subject = messageDto.Subject;
            message.Body = bodyBuilder.ToMessageBody();

            using var _smtpClient = new SmtpClient();
            await _smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await _smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
            await _smtpClient.SendAsync(message);
            await _smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailAsync), ex);
        }
        finally
        {
            _clients.Enqueue(_smtpClient);
        }

    }

    /// <summary>
    ///  Envio de corrro con archivo en bytes 
    /// </summary>
    /// <param name="messageDto"></param>
    /// <param name="dataFile"></param>
    /// <param name="fileName"></param>
    /// <param name="readReceipt">Indica si se solicitará una confirmación de lectura</param>
    /// <returns>Una tarea representando la operación asíncrona.</returns>
    public async Task SendEmailAsync(EmailMessageDto messageDto, byte[] dataFile, string fileName, bool? readReceipt = false)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("WebAppBase", _smtpUser));

            // Manejo de múltiples destinatarios separados por ';'
            var recipients = messageDto.Destination.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(e => e.Trim())
                                                   .Distinct();

            foreach (var recipient in recipients)
            {
                message.To.Add(new MailboxAddress(null, recipient));
            }

            // Crear el cuerpo del correo
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = messageDto.Body, // Cuerpo del correo en HTML
            };



            if (Convert.ToBoolean(readReceipt))
            {
                // Agregar encabezado de confirmación de lectura. Nota: en Gmail sólo funciona para cuentas empresariales.
                message.Headers.Add("Disposition-Notification-To", _smtpUser);
            }

            // Agregar el archivo como adjunto desde los bytes
            var contentType = new ContentType("application", "pdf");
            using (var stream = new MemoryStream(dataFile))
            {
                bodyBuilder.Attachments.Add(fileName + ".pdf", stream, contentType);
            }

            message.Subject = messageDto.Subject;
            message.Body = bodyBuilder.ToMessageBody();

            using var _smtpClient = new SmtpClient();
            await _smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await _smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
            await _smtpClient.SendAsync(message);
            await _smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(SendEmailAsync), ex);
        }
        finally
        {
            _clients.Enqueue(_smtpClient);
        }

    }
}
