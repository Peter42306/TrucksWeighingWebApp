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
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ApplicationUserId, o => o.Ignore())
                .ForMember(d => d.ApplicationUser, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.TruckRecords, o => o.Ignore());

            // Edit: VM -> Entity
            CreateMap<InspectionEditViewModel, Inspection>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.ApplicationUserId, o => o.Ignore())
                .ForMember(d => d.ApplicationUser, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.TruckRecords, o => o.Ignore());

            // Entity -> Edit VM
            CreateMap<Inspection, InspectionEditViewModel>();
        }                
    }    

    public class TruckRecordProfile : Profile
    {
        public TruckRecordProfile()
        {
            CreateMap<TruckRecordCreateViewModel, TruckRecord>()
                .ForMember(d => d.Id, o => o.Ignore())                
                .ForMember(d => d.Inspection, o => o.Ignore())
                .ForMember(d => d.PlateNumber, o => o.MapFrom(s => s.PlateNumber.Trim().ToUpperInvariant()));

            CreateMap<TruckRecordEditViewModel, TruckRecord>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.InspectionId, o => o.Ignore())
                .ForMember(d => d.Inspection, o => o.Ignore())
                .ForMember(d => d.SerialNumber, o => o.Ignore())
                .ForMember(d => d.PlateNumber, o => o.MapFrom(s => s.PlateNumber.Trim().ToUpperInvariant()));
            
            CreateMap<TruckRecord, TruckRecordEditViewModel>();
        }
    }
}
