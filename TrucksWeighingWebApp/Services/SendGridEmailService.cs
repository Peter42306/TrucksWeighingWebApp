using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Services
{
    public class SendGridEmailService : IEmailSender
    {
        private readonly SendGridOptions _sendGridOptions;        
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IOptions<SendGridOptions> sendGridOptions, ILogger<SendGridEmailService> logger)
        {
            _sendGridOptions = sendGridOptions.Value;
            _logger=logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {            
            _logger.LogWarning("SendGrid: sending to={Email}, subj={Subj}", email, subject);

            if (string.IsNullOrWhiteSpace(_sendGridOptions.ApiKey))
            {
                throw new Exception("SendGrid API key is not configured.");
            }

            var client = new SendGridClient(_sendGridOptions.ApiKey);

            var from = new EmailAddress(_sendGridOptions.FromEmail, _sendGridOptions.FromName);

            var to = new EmailAddress(email);

            var plainText = System.Text.RegularExpressions.Regex.Replace(htmlMessage, "<.*?>", string.Empty);            

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlMessage);

            if (htmlMessage.Contains("From:") && htmlMessage.Contains("@"))
            {
                msg.ReplyTo = new EmailAddress(email);
            }

            var response = await client.SendEmailAsync(msg);

            var bodyText = await response.Body.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {               
                _logger.LogError("SendGrid error {Status}: {Body}", response.StatusCode, bodyText);
                throw new Exception($"SendGrid error {response.StatusCode}: {bodyText}");
            }

            _logger.LogInformation("SendGrid: delivered {Status}", response.StatusCode);
        }
    }
}
