using TrucksWeighingWebApp.Models;
using TrucksWeighingWebApp.ViewModels;

namespace TrucksWeighingWebApp.DTOs.Export
{
    public record TrucksExcelDto
    {
        public Inspection Inspection { get; init; } = null!;
        public bool ShowTimes { get; init; }
        public PeriodStatsViewModel? PeriodStats { get; init; }
        public List<TruckRowDto> RowsDto { get; init; } = new();
    }
}
