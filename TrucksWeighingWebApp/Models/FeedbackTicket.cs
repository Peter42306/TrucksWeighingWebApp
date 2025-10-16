using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrucksWeighingWebApp.Models
{
    public class FeedbackTicket
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public string UserEmail { get; set; } = null!;
        public string Message { get; set; } = null!;

        public DateTime CreatedUtc { get; set; } // default value in db
        public string? AdminNotes { get; set; }
    }
}
