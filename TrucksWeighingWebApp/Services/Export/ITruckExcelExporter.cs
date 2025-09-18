using TrucksWeighingWebApp.DTOs.Export;

namespace TrucksWeighingWebApp.Services.Export
{
    public interface ITruckExcelExporter
    {
        byte[] BuildTruckWorkbook(TrucksExcelDto dto);
    }
}
