using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class InspectionEditViewModel
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string? Vessel { get; set; }

        [StringLength(128)]
        public string? Cargo { get; set; }

        [StringLength(128)]
        public string? Place { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Value should be between 0 and 1,000,000,000")]
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DeclaredTotalWeight { get; set; }
                
        public string TimeZoneId { get; set; } = "UTC";
    }
}
