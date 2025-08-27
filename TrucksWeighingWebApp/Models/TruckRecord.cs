namespace TrucksWeighingWebApp.Models
{
    public class TruckRecord
    {
        public int Id { get; set; }        
        public int InspectionId { get; set; }
        public required Inspection Inspection { get; set; }

        
        public string PlateNumber { get; set; } = string.Empty;
        
        public DateTime InitialWeightAtUtc { get; set; }
        public decimal InitialWeight { get; set; }
        
        public DateTime FinalWeightAtUtc { get; set; }
        public decimal FinalWeight { get; set; }

        public decimal NetWeight { get; set; }        
    }    
}
