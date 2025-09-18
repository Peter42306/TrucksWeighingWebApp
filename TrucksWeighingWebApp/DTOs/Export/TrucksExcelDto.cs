using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.DTOs.Export
{
    public record TrucksExcelDto
    {
        public Inspection Inspection { get; init; } = null!;
        public bool ShowTimes { get; init; }
        public List<TruckRowDto> RowsDto { get; init; } = new();
    }
}
