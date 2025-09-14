using System.ComponentModel.DataAnnotations;
using TrucksWeighingWebApp.Infrastructure.TimeZone;

namespace TrucksWeighingWebApp.ViewModels
{
    public class WeighRangeFilterViewModel : IValidatableObject
    {
        public DateTime? FromLocal { get; set; }
        public DateTime? ToLocal { get; set; }

        public bool IncludeWeighingTimes { get; set; } // to PDF export

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FromLocal.HasValue && ToLocal.HasValue && FromLocal > ToLocal)
            {
                yield return new ValidationResult(
                    "Initial Weighing cannot be later than Final Weighing",
                    new[] { nameof(FromLocal), nameof(ToLocal) });
            }
        }

        public (DateTime? fromUtc, DateTime? toUtc) ToUtc(string timeZoneId)
        {
            if (FromLocal == null && ToLocal == null)
            {
                return (null, null);
            }

            var tz = Tz.Get(timeZoneId ?? "UTC");
            



            DateTime? from = FromLocal.HasValue
                ? Tz.ToUtc(FromLocal.Value, tz)                
                : (DateTime?)null;

            DateTime? to = ToLocal.HasValue
                ? Tz.ToUtc(ToLocal.Value, tz)
                : (DateTime?)null;

            return (from, to);
        }



    }
}
