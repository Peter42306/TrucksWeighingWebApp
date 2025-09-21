using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.Models
{
    public enum LogoPosition
    {
        Left,
        Center,
        Right
    }

    public class UserLogo
    {
        public int Id { get; set; }

        [Required]
        public required string ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; } // logo name

        [Range(20, 100)]
        public int Height { get; set; } = 50; // logo height in pdf header document for QuestPDF

        [Range(1, 100)]
        public int PaddingBottom { get; set; } = 20; // space beneath logo for QuestPDF

        public LogoPosition Position { get; set; } = LogoPosition.Left;

        [Required]
        public required byte[] ImageBytes { get; set; }

        [Required, MaxLength(64)]
        public required string ContentType { get; set; } // "image/png" | "image/jpeg" | "image/jpg"

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
