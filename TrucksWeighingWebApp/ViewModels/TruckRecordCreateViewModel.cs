using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class TruckRecordCreateViewModel
    {
        [Required]
        public int InspectionId { get; set; }

        [StringLength(64)]
        public required string PlateNumber { get; set; }

        public DateTime? InitialWeightAtUtc { get; set; }


        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Value should be between 0 and 1,000,000,000")]
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? InitialWeight { get; set; }


        public DateTime? FinalWeightAtUtc { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Value should be between 0 and 1,000,000,000")]
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FinalWeight { get; set; }
    }
}
