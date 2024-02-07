using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

namespace ImmunizNation.Client.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        public MailKitEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(_settings.EmailFrom));
            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(_settings.SmtpUser, _settings.SmtpPassword);
                smtp.Send(emailMessage);
                smtp.Disconnect(true);
            }

            return Task.FromResult(true);
        }
    }
}
