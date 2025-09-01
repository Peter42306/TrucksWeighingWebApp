namespace TrucksWeighingWebApp.Models
{
    public class UserSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = default!;
        public DateTime StartedUtc { get; set; }
        public DateTime LastSeenUtc { get; set; }
        public bool IsClosed { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
    }
}
