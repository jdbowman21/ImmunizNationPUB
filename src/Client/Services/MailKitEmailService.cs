using ImmunizNation.Client.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImmunizNation.Client.Services
{
    /// <summary>
    /// Interface for sending emails with attachments
    /// </summary>
    public interface IEmailService
    {
        void Send(string to, string subject, string html, IEnumerable<EmailAttachment> attachments = null);
    }

    public class MailKitEmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public MailKitEmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public void Send(string email, string subject, string htmlMessage, IEnumerable<EmailAttachment> attachments = null)
        {
            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(_settings.EmailFrom));
            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;
            
            if(attachments != null)
            {
                foreach(var attachment in attachments)
                {
                    builder.Attachments.Add(attachment.Name, attachment.ByteArray, attachment.ContentType);       
                }
            }

            emailMessage.Body = builder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
               

                smtp.Connect(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
                smtp.Authenticate(_settings.SmtpUser, _settings.SmtpPassword);
                smtp.Send(emailMessage);
                smtp.Disconnect(true);
            }
        }
    }
}
