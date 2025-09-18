using System.ComponentModel.DataAnnotations;

namespace TrucksWeighingWebApp.ViewModels
{
    public class TruckRecordsExportToExcelViewModel : IValidatableObject
    {
        public int InspectionId { get; set; }

        public bool IncludeTimes { get; set; } = true; // show times columns in exported file
        public bool PrintAll { get; set; } = true; // export all 

        public WeighRangeFilterViewModel WeighRangeFilter { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!PrintAll)
            {
                foreach (var dateTimeInPeriod in WeighRangeFilter.Validate(validationContext))
                {
                    yield return dateTimeInPeriod;
                }
            }
        }
    }
}
