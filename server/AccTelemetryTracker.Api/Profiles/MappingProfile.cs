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
        
        CreateMap<MotecFile, MotecFileDto>()
            .ForMember(d => d.Username, opt => opt.MapFrom(m => m.User.ServerName));


        CreateMap<Car, CarDto>();

        CreateMap<Track, TrackDto>();

        CreateMap<User, UserDto>()
            .ForMember(d => d.FileUploadCount, opt => opt.MapFrom(u => u.MotecFiles.Count));

        CreateMap<Logic.MotecFile, MotecLapDto>();

        CreateMap<AverageLap, AverageLapDto>()
            .ForMember(d => d.Car, opt => opt.MapFrom(m => m.Car.Name))
            .ForMember(d => d.TrackName, opt => opt.MapFrom(m => m.Track.Name))
            .ForMember(d => d.FastestLap, opt => opt.MapFrom(m => m.AverageFastestLap));

        CreateMap<Audit, AuditDto>()
            .ForMember(a => a.EventType, opt => opt.MapFrom(au => au.EventType.ToString()))
            .ForMember(a => a.Username, opt => opt.MapFrom(au => au.User.ServerName));
    }
}