using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class InspectionIndexViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "User Email")] 
        public string? ApplicationUser { get; set; }

        [Display(Name = "Vessel")]
        public string? Vessel { get; set; }

        [Display(Name = "Cargo")]
        public string? Cargo { get; set; }

        [Display(Name = "Port")]
        public string? Place { get; set; }

        [Display(Name = "Declared weight")]
        public decimal? DeclaredTotalWeight { get; set; }
                
        public DateTime CreatedAtUtc { get; set; }

        [Display(Name = "Time zone")]
        public string TimeZoneId { get; set; } = "UTC";

        [Display(Name = "Created")]
        public DateTime CreatedAtLocal { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
