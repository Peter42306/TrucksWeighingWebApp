using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.ViewModels
{
    public static class PageSizesTrucksStatus
    {
        public const int Small = 5;
        public const int Default = 10;
        public const int Large = 50;
        public const int VeryLarge = 100;
        public const int All = int.MaxValue;

        public static readonly IReadOnlyList<(int Value, string Label)> PageSizesTrucksStatusOptions = new List<(int, string)>
        {
            (Small, "5"),
            (Default, "10"),
            (Large, "50"),
            (VeryLarge, "100"),
            (All, "All")
        };
    }

    public class TruckRecordPierIndexViewModel
    {
        public required Inspection Inspection { get; set; }

        public IReadOnlyList<TruckRecord> AfterInitialWeighingToCargoOps { get; init; } = Array.Empty<TruckRecord>();
        public IReadOnlyList<TruckRecord> UnderCargoOperations { get; init; } = Array.Empty<TruckRecord>();
        public IReadOnlyList<TruckRecord> AfterCargoOpsToFinalWeighing { get; init; } = Array.Empty<TruckRecord>();
        public IReadOnlyList<TruckSummaryDto> AfterFinalWeighing { get; init; } = Array.Empty<TruckSummaryDto>();


        // pagination for table AfterFinalWeighing
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PageSizesTrucksStatus.Default;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; } = 0;
        
        public IReadOnlyList<(int Value, string Label)> PageSizeOptions
        {
            get
            {
                return PageSizesTrucksStatus.PageSizesTrucksStatusOptions;
            }
        }



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
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var inspection = await db.Inspections
                .AsNoTracking()
                .FirstAsync(i => i.Id == inspectionId, ct);

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



            var allowed = PageSizesTrucksStatus.PageSizesTrucksStatusOptions
                .Select(i => i.Value)
                .ToHashSet();

            if (!allowed.Contains(pageSize))
            {
                pageSize = PageSizesTrucksStatus.Default;
            }

            var afterFinalQuery = query.Where(t => t.FinalWeightAtUtc != null);

            var total = await afterFinalQuery.CountAsync(ct);                
                
            var showAll = pageSize == PageSizesTrucksStatus.All;

            int totalPages = showAll
                ? 1
                : Math.Max(1, (int)Math.Ceiling(total/(double)pageSize));

            int currentPage = showAll
                ? 1 
                : Math.Min(Math.Max(1, page), totalPages);


            IQueryable<TruckRecord> pageQuery = afterFinalQuery.OrderByDescending(t => t.FinalWeightAtUtc);

            if (!showAll)
            {
                pageQuery = pageQuery
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);
            }


            var afterFinalWeighing = await pageQuery
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

                AfterFinalWeighing = afterFinalWeighing,
                Page = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = total
                
            };

        }
    }
}
