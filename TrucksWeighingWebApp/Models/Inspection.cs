using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrucksWeighingWebApp.Models
{
    public class Inspection
    {
        public int Id { get; set; }

        public required string ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }


        public string? Vessel { get; set; }
        public string? Cargo { get; set; }
        public string? Place { get; set; }                
        public decimal? DeclaredTotalWeight { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string TimeZoneId { get; set; } = "UTC";

        public string? Notes { get; set; }

        public ICollection<TruckRecord> TruckRecords { get; set; } = new List<TruckRecord>();


        // Results
        [NotMapped]
        public decimal WeighedTotalWeight
        {
            get { return CalculateWeighedTotalWeight(); }
        }

        [NotMapped]
        public decimal DifferenceWeight 
        {
            get { return CalculateDifferenceWeight(); }
        }

        [NotMapped]
        public decimal DifferencePercent 
        {
            get { return CalculateDifferencePercent(); }
        }

        
        
        
        public decimal CalculateWeighedTotalWeight()
        {
            if (TruckRecords.Count == 0)
            {
                return 0m;
            }

            decimal sum = 0m;
            foreach (var truckRecord in TruckRecords)
            {
                sum += truckRecord.NetWeight;
            }

            return sum;
        }

        public decimal CalculateDifferenceWeight()
        {
            if (TruckRecords.Count == 0 || !DeclaredTotalWeight.HasValue)
            {
                return 0m;
            }

            decimal weighed = CalculateWeighedTotalWeight();
            decimal differenceWeight = weighed - DeclaredTotalWeight.Value;

            return differenceWeight;
        }

        public decimal CalculateDifferencePercent()
        {
            if (!DeclaredTotalWeight.HasValue || DeclaredTotalWeight.Value == 0)
            {
                return 0m;
            }

            decimal differencePercent = 100*DifferenceWeight/DeclaredTotalWeight.Value;

            return differencePercent;
        }
    }
}
