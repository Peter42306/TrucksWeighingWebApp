using System.Collections;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.ViewModels
{
    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public static class PageSizes
    {
        public const int Small = 5;
        public const int Default = 10;
        public const int Large = 50;
        public const int VeryLarge = 100;
        public const int All = int.MaxValue;

        public static IReadOnlyList<(int Value, string Label)> Options = new List<(int, string)>
        {
            (Small, "5"),
            (Default, "10"),            
            (Large, "50"),
            (VeryLarge, "100"),
            (All, "All")
        };
    }

    public class TruckRecordIndexViewModel
    {
        public required Inspection Inspection { get; set; }
        
        public TruckRecordCreateViewModel New {  get; set; } =
            new TruckRecordCreateViewModel() 
            {
                PlateNumber = string.Empty
            };

        public TruckRecordEditViewModel? Edit { get; set; }

        public SortOrder SortOrder { get; set; } = SortOrder.Ascending; // "asc" | "desc"


        public IReadOnlyList<TruckRecord> TruckRecords { get; set; } = Array.Empty<TruckRecord>();
        

        // pagination
        public int Page { get; set; } = 1;          // 1-based
        public int PageSize { get; set; } = PageSizes.Default;     // 10, 50, "All" = int.MaxValue
        public int TotalCount { get; set; }
        public int TotalPages
        {
            get
            {
                if (PageSize >= PageSizes.All)
                {
                    return 1;
                }
                else
                {
                    double result = (double)TotalCount / PageSize;
                    return (int)Math.Ceiling(result);
                }
            }
        }

        public IReadOnlyList<(int Value, string Label)> PageSizeOptions
        {
            get
            {
                return PageSizes.Options;
            }
        }

    }
}
