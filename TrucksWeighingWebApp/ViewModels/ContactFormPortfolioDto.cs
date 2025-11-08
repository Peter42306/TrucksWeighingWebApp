using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class ContactFormPortfolioDto
    {
        [Required]
        public string Name { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Subject { get; set; } = default!;

        [Required]
        public string Message { get; set; } = default!;
    }
}
