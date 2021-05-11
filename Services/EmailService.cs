using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using MimeKit;
using MimeKit.Utils;

namespace HackerRank.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string callbackurl, string username);
    }

    public class EmailService : IEmailSender
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public EmailService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string email, string subject, string callbackurl, string username)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("HackerRank", "hacker@hackerrank.se"));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = subject;

            BodyBuilder builder = new();
            using StreamReader reader = new(Path.Combine(_webHostEnvironment.ContentRootPath, "Services", "Mail", "emailTemplate.html"));
            string body = reader.ReadToEnd();

            var logo = builder.LinkedResources.Add(Path.Combine(_webHostEnvironment.WebRootPath, "img", "logo.png"));
            var icon = builder.LinkedResources.Add(Path.Combine(_webHostEnvironment.WebRootPath, "img", "emailIcon.png"));
            logo.ContentId = MimeUtils.GenerateMessageId();
            icon.ContentId = MimeUtils.GenerateMessageId();

            body = body.Replace("{CALLBRACK_URL_HERE}", callbackurl);
            body = body.Replace("{LOGO_HERE}", "cid:" + logo.ContentId);
            body = body.Replace("{ICON_HERE}", "cid:" + icon.ContentId);
            body = body.Replace("{COPYRIGHT_HERE}", "HackerRank");
            builder.HtmlBody = body;

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp-relay.sendinblue.com", 587, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_configuration["Authentication-MailTrap-ClientId"], _configuration["Authentication-MailTrap-ClientSecret"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
