using System.Net;
using System.Net.Mail;

namespace ASP.Services.Email
{
    public class GmailService(IConfiguration configuration) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;

        public void Send(string to, string subject, string content)
        {
            var emailSection = _configuration.GetSection("Email") ?? throw new Exception("Email section not found");
            var gmailSection = emailSection.GetSection("Gmail") ?? throw new Exception("Email.Gmail section not found");
            String host = gmailSection.GetSection("Host")?.Value ?? throw new Exception("Email.Gmail.Host section not found");
            int port = gmailSection.GetSection("Port")?.Get<int>() ?? throw new Exception("Email.Gmail.Port section not found");
            String box = gmailSection.GetSection("Box")?.Value ?? throw new Exception("Email.Gmail.Box section not found");
            String appKey = gmailSection.GetSection("AppKey")?.Value ?? throw new Exception("Email.Gmail.AppKey section not found");

            using SmtpClient smtpClient = new(host)
            {
                Port = port,
                EnableSsl = true,
                Credentials = new NetworkCredential(box, appKey)
            };
            smtpClient.Send(box, to, subject, content);
        }
    }
}
