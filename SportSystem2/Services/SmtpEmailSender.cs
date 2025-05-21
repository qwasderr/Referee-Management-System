using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
namespace SportSystem2.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailSender(SmtpSettings smtpSettings)
        {
            _smtpSettings = smtpSettings;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.From),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
    }
}
