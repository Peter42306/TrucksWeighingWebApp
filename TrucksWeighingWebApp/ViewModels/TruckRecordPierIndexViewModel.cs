using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.ViewModels
{
    public class TruckRecordPierIndexViewModel
    {
        public required Inspection Inspection { get; set; }

        public IReadOnlyList<TruckRecord> AfterInitialWeighingToCargoOps { get; init; } = Array.Empty<TruckRecord>();
        public IReadOnlyList<TruckRecord> UnderCargoOperations { get; init; } = Array.Empty<TruckRecord>();
        public IReadOnlyList<TruckRecord> AfterCargoOpsToFinalWeighing { get; init; } = Array.Empty<TruckRecord>();
        public IReadOnlyList<TruckSummaryDto> AfterFinalWeighing { get; init; } = Array.Empty<TruckSummaryDto>();


        public sealed record TruckSummaryDto(
            int Id,
            int SerialNumber,
            string PlateNumber,

            DateTime? InitialWeighing,
            DateTime? CargoOpsStarted,
            DateTime? CargoOpsFinished,
            DateTime? FinalWeighing,

            TimeSpan? DurationInitialWeighingToCargoOpsStarted,
            TimeSpan? DurationCargoOpsStartedToCargoOpsFinished,
            TimeSpan? DurationCargoOpsFinishedToFinalWeighing,
            TimeSpan? DurationTotal
            );

        public static async Task<TruckRecordPierIndexViewModel> CreateAsync(
            ApplicationDbContext db,
            int inspectionId,
            CancellationToken ct)
        {
            var inspection = await db.Inspections.AsNoTracking().FirstAsync(i => i.Id == inspectionId, ct);

            var query = db.TruckRecords.AsNoTracking().Where(t => t.InspectionId == inspectionId);

            var afterInitialWeighingToCargoOps = await query
                .Where(t => t.InitialWeightAtUtc != null && t.InitialBerthAtUtc == null)
                .OrderByDescending(t => t.InitialWeightAtUtc)
                .ToListAsync(ct);

            var underCargoOperations = await query
                .Where(t => t.InitialBerthAtUtc != null && t.FinalBerthAtUtc == null)
                .OrderByDescending(t => t.InitialBerthAtUtc)
                .ToListAsync(ct);

            var afterCargoOpsToFinalWeighing = await query
                .Where(t => t.FinalBerthAtUtc != null && t.FinalWeightAtUtc == null)
                .OrderByDescending(t => t.FinalBerthAtUtc)
                .ToListAsync(ct);

            var afterFinalWeighing = await query
                .Where(t => t.FinalWeightAtUtc != null)
                .OrderByDescending(t => t.FinalWeightAtUtc)
                .Select(t => new TruckRecordPierIndexViewModel.TruckSummaryDto(
                    t.Id,
                    t.SerialNumber,
                    t.PlateNumber,
                    t.InitialWeightAtUtc,
                    t.InitialBerthAtUtc,
                    t.FinalBerthAtUtc,
                    t.FinalWeightAtUtc,
                    t.InitialBerthAtUtc != null && t.InitialWeightAtUtc != null ? t.InitialBerthAtUtc - t.InitialWeightAtUtc : null,
                    t.InitialBerthAtUtc != null && t.FinalBerthAtUtc != null ? t.FinalBerthAtUtc - t.InitialBerthAtUtc : null,
                    t.FinalBerthAtUtc != null && t.FinalWeightAtUtc != null ? t.FinalWeightAtUtc - t.FinalBerthAtUtc : null,
                    t.FinalWeightAtUtc != null && t.InitialWeightAtUtc != null ? t.FinalWeightAtUtc - t.InitialWeightAtUtc : null))
                .ToListAsync(ct);

            return new TruckRecordPierIndexViewModel
            {
                Inspection = inspection,
                AfterInitialWeighingToCargoOps = afterInitialWeighingToCargoOps,
                UnderCargoOperations = underCargoOperations,
                AfterCargoOpsToFinalWeighing = afterCargoOpsToFinalWeighing,
                AfterFinalWeighing = afterFinalWeighing
            };

        }
    }
}
