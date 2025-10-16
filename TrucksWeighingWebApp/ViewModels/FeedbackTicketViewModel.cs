using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class FeedbackTicketViewModel
    {
        [Required, StringLength(4000, MinimumLength = 5)]
        public string Message { get; set; } = string.Empty;
    }
}
