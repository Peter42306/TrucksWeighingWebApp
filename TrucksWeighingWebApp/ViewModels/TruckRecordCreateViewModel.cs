using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class TruckRecordCreateViewModel
    {
        [Required]
        public int InspectionId { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Plate number")]
        public string PlateNumber { get; set; } = string.Empty;

        [Display(Name = "Initial weight time")]
        public DateTime? InitialWeightAtUtc { get; set; }

        [Display(Name = "Initial weight")]
        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Value should be between 0 and 1,000,000,000")]        
        public decimal? InitialWeight { get; set; }

        [Display(Name = "Final weight time")]
        public DateTime? FinalWeightAtUtc { get; set; }

        [Display(Name = "Final weight")]
        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Value should be between 0 and 1,000,000,000")]        
        public decimal? FinalWeight { get; set; }
    }
}
