using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Helpers.Services
{
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private EmailSenderSettings _settings { get; set; }

        public EmailSender(Microsoft.Extensions.Options.IOptions<EmailSenderSettings> settings) {
            _settings = settings.Value;
        }

        public Task SendEmailAsync(string from, string to, string subject, string message)
        {
            SmtpClient client = new SmtpClient(_settings.host);
            client.UseDefaultCredentials = false;
            client.Port = _settings.port;
            client.EnableSsl = _settings.enableSSL;
            client.Credentials = new NetworkCredential(_settings.username, _settings.password);

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(from);

            mailMessage.To.Add(string.Format(to));
            mailMessage.Body = message;
            mailMessage.Subject = subject;

            client.Send(mailMessage);

            return Task.CompletedTask;
        }
    }
}
