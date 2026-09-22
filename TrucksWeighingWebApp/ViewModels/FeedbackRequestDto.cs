namespace TrucksWeighingWebApp.ViewModels
{
    public sealed class FeedbackRequestDto
    {
        public string AppKey { get; init; } = string.Empty;
        public string? UserId { get; init; }
        public string? SenderEmail { get; init; }
        public int Type { get; init; } = 1; // 1=General
        public string? Subject { get; init; }
        public string Body { get; init; } = string.Empty;
    }
}
