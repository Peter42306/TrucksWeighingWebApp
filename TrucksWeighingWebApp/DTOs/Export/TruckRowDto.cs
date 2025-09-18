namespace TrucksWeighingWebApp.DTOs.Export
{
    public record TruckRowDto
    {
        public int SerialNumber { get; init; }
        public string PlateNumber { get; init; } = string.Empty;

        public decimal? InitialWeight { get; init; }
        public DateTime? InitialWeighingLocal { get; init; } // if Showtimes=false --> null

        public decimal? FinalWeight { get; init; }
        public DateTime? FinalWeighingLocal { get; init; } // if Showtimes=false --> null        
        
        public decimal NetWeight { get; init; }
    }
}
