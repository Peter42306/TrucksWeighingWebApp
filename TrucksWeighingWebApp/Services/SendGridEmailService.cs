using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TrucksWeighingWebApp.Services
{
    public class SendGridEmailService : IEmailSender
    {
        private readonly IConfiguration _config;

        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _config["SendGrid:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("SendGrid API key is not configured.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(
                _config["SendGrid:FromEmail"] ?? "no-reply@zalizko.site",
                _config["SendGrid:FromName"] ?? "Trucks Weighing Web App"
            );           
            
            var to = new EmailAddress(email);
            var plainText = System.Text.RegularExpressions.Regex.Replace(htmlMessage, "<.*?>", string.Empty);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlMessage);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStreamAsync();
                throw new Exception($"SendGrid error {response.StatusCode}: {body}");
            }            
        }
    }
}
