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
        public string ApplicationUserId { get; set; } = default!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        [Required, MaxLength(100)]
        public required string Name { get; set; } // logo name

        [Range(20, 100)]
        public int Height { get; set; } = DefaultHeight; // logo height in pdf header document for QuestPDF

        [Range(1, 100)]
        public int PaddingBottom { get; set; } = DefaultPaddingBottom; // space beneath logo for QuestPDF

        public LogoPosition Position { get; set; } = DefaultPosition; // logo position for QuestPDF

        [Required, MaxLength(512)]
        public required string FilePath { get; set; }

        [Range(0, long.MaxValue)]
        public long FileSizeBytes { get; set; }

        [MaxLength(250)]
        public string? OriginalFileName { get; set; }


        [Required, MaxLength(64)]
        public required string ContentType { get; set; } // "image/png" | "image/jpeg"

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;


        // Default constant values
        public const int DefaultHeight = 50;
        public const int DefaultPaddingBottom = 20;
        public const LogoPosition DefaultPosition = LogoPosition.Left;
    }
}
