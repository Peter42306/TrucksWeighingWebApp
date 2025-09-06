using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class TruckRecordEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int InspectionId { get; set; }

        [Required, StringLength(64)]
        public string PlateNumber { get; set; } = string.Empty;

        
        public DateTime? InitialWeightAtUtc { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Value should be between 0 and 1,000,000,000")]        
        public decimal? InitialWeight { get; set; }

        
        public DateTime? FinalWeightAtUtc { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Value should be between 0 and 1,000,000,000")]        
        public decimal? FinalWeight { get; set; }
    }
}
