using AutoMapper;
using TrucksWeighingWebApp.Models;
using TrucksWeighingWebApp.ViewModels;

namespace TrucksWeighingWebApp.Mappings
{
    public class InspectionProfile : Profile
    {
        public InspectionProfile() 
        {
            // Create
            CreateMap<InspectionCreateViewModel, Inspection>()
                .ForMember(d => d.Id,                   o => o.Ignore())
                .ForMember(d => d.InspectorId,          o => o.Ignore())
                .ForMember(d => d.Inspector,            o => o.Ignore())
                .ForMember(d => d.CreatedAt,            o => o.Ignore())
                .ForMember(d => d.TruckRecords,         o => o.Ignore())
                .ForMember(d => d.WeighedTotalWeight,   o => o.Ignore())
                .ForMember(d => d.DifferenceWeight,     o => o.Ignore())
                .ForMember(d => d.DifferencePercent,    o => o.Ignore());

            // Edit
            CreateMap<InspectionEditViewModel, Inspection>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.InspectorId, o => o.Ignore())
                .ForMember(d => d.Inspector, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.TruckRecords, o => o.Ignore())
                .ForMember(d => d.WeighedTotalWeight, o => o.Ignore())
                .ForMember(d => d.DifferenceWeight, o => o.Ignore())
                .ForMember(d => d.DifferencePercent, o => o.Ignore());

            // For filling Edit view
            CreateMap<Inspection, InspectionEditViewModel>();
        }        
    }

    //public class MappingProfile : Profile
    //{
    //    public MappingProfile()
    //    {
    //        // Create Inspection
    //        CreateMap<InspectionCreateViewModel, Inspection>()
    //            .ForMember();
    //    }
    //}
}
