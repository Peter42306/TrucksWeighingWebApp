using System.Collections;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.ViewModels
{
    public class TruckRecordIndexViewModel
    {
        public required Inspection Inspection { get; set; }
        public TruckRecordCreateViewModel New {  get; set; } =
            new TruckRecordCreateViewModel() 
            {
                PlateNumber = string.Empty
            };

        public IEnumerable<TruckRecord> TruckRecords
        {
            get
            {
                if (Inspection != null && Inspection.TruckRecords != null)
                {
                    return Inspection.TruckRecords;
                }
                else
                {
                    return Enumerable.Empty<TruckRecord>();
                }
            }
        }
    }
}
