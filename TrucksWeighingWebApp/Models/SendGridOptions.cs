namespace TrucksWeighingWebApp.Models
{
    public sealed class SendGridOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Trucks Weighing Web App";
    }
}
