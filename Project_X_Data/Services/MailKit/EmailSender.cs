using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
namespace Project_X_Data.Services.MailKit
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();

            client.Connect(
                _configuration["Email:SmtpServer"],
                int.Parse(_configuration["Email:Port"]),
                true 
            );

            client.Authenticate(
                _configuration["Email:Username"],
                _configuration["Email:Password"]
            );

            client.Send(message);
            client.Disconnect(true);
        }
    }
}
