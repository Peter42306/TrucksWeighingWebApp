using System.ComponentModel.DataAnnotations;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.ViewModels
{
    public class InspectionEditViewModel
    {
        [Required]
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

        [StringLength(2000)]
        public string? Notes { get; set; }
        
        public int? UserLogoId { get; set; }
        
        //public List<(int Id, string Text, string FilePath)> LogoOptions = new();

        public List<LogoOptionsViewModel> LogoOptions { get; set; } = new();

        //public class LogoOptionVm
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; } = string.Empty;
        //    public int Height { get; set; }
        //    public int PaddingBottom { get; set; }
        //    public LogoPosition Position { get; set; }
        //    public string FilePath { get; set; } = string.Empty;
        //}
    }
}
