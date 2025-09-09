using System.ComponentModel.DataAnnotations.Schema;

namespace TrucksWeighingWebApp.Models
{
    public class TruckRecord
    {
        public int Id { get; set; }        

        public int InspectionId { get; set; }
        public required Inspection Inspection { get; set; }

        
        public string PlateNumber { get; set; } = string.Empty;
        public int SerialNumber { get; set; }

        
        public DateTime? InitialWeightAtUtc { get; set; }
        public decimal? InitialWeight { get; set; }
        
        public DateTime? FinalWeightAtUtc { get; set; }
        public decimal? FinalWeight { get; set; }

        // Results
        [NotMapped]
        public decimal NetWeight 
        {
            get { return CalculateNetWeight(); }
        }        



        public decimal CalculateNetWeight()
        {
            if (!InitialWeight.HasValue || !FinalWeight.HasValue)
            {
                return 0m;
            }

            decimal netWeight = Math.Abs(InitialWeight.Value - FinalWeight.Value);

            return netWeight;
        }
    }    
}
