using FixConnect.BLL.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FixConnect.BLL.Services
{
    public class EmailSender
    {
        private readonly EmailSettings _settings;

        // ✅ DI: EmailSettings injected via IOptions<EmailSettings>
        public EmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public void SendEmail(string toEmail, string subject, string htmlBody)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
                EnableSsl = true
            };

            smtpClient.Send(message);
        }

        // Async version — recommended to use this one from AuthService
        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}