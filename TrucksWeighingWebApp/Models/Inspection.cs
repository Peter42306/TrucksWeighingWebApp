namespace TrucksWeighingWebApp.Models
{
    public class Inspection
    {
        public int Id { get; set; }

        public required string InspectorId { get; set; }
        public required ApplicationUser Inspector { get; set; }

        public string? Vessel { get; set; }
        public string? Cargo { get; set; }
        public string? Place { get; set; }
        public decimal? DeclaredTotalWeight { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TimeZoneId { get; set; } = "UTC";
        
        public List<TruckRecord> TruckRecords { get; set; } = new List<TruckRecord>();

        public decimal? WeighedTotalWeight { get; set; }
        public decimal? DifferenceWeight { get; set; }
        public decimal? DifferencePercent { get; set; }
    }
}
