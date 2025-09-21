namespace TrucksWeighingWebApp.ViewModels
{
    public sealed class PeriodStatsViewModel
    {
        public int Trucks { get; set; }
        public decimal Weight { get; set; }

        public DateTime? FromLocal { get; set; }
        public DateTime? ToLocal { get; set; }

        public bool IsSelected
        {
            get
            {
                return FromLocal.HasValue || ToLocal.HasValue;
            }
        }
    }
}
