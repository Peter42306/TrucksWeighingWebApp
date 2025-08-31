using AutoMapper;
using TrucksWeighingWebApp.Models;
using TrucksWeighingWebApp.ViewModels;

namespace TrucksWeighingWebApp.Mappings
{
    public class InspectionProfile : Profile
    {
        public InspectionProfile() 
        {
            // Create: VM -> Entity
            CreateMap<InspectionCreateViewModel, Inspection>()
                .ForMember(d => d.Id,                   o => o.Ignore())
                .ForMember(d => d.UserId,          o => o.Ignore())
                .ForMember(d => d.User,            o => o.Ignore())
                .ForMember(d => d.CreatedAt,            o => o.Ignore())
                .ForMember(d => d.TruckRecords,         o => o.Ignore())
                .ForMember(d => d.WeighedTotalWeight,   o => o.Ignore())
                .ForMember(d => d.DifferenceWeight,     o => o.Ignore())
                .ForMember(d => d.DifferencePercent,    o => o.Ignore());

            // Edit: VM -> Entity
            CreateMap<InspectionEditViewModel, Inspection>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.TruckRecords, o => o.Ignore())
                .ForMember(d => d.WeighedTotalWeight, o => o.Ignore())
                .ForMember(d => d.DifferenceWeight, o => o.Ignore())
                .ForMember(d => d.DifferencePercent, o => o.Ignore());

            // Entity -> Edit VM
            CreateMap<Inspection, InspectionEditViewModel>();
        }        
    }    
}
