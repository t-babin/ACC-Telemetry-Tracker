using AccTelemetryTracker.Api.Dto;
using AccTelemetryTracker.Datastore.Models;
using AutoMapper;

namespace AccTelemetryTracker.Api.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<MotecFile, MotecFileDto>()
            .ForMember(d => d.CarName, opt => opt.MapFrom(m => m.Car.Name));
            
        CreateMap<MotecFile, MotecFileDto>()
            .ForMember(d => d.CarClass, opt => opt.MapFrom(m => m.Car.Class));
            
        CreateMap<MotecFile, MotecFileDto>()
            .ForMember(d => d.TrackName, opt => opt.MapFrom(m => m.Track.Name));
    }
}