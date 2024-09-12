using ApiApplication.Database.Entities;
using ApiApplication.DTOs.MovieDTOs;
using ApiApplication.DTOs.SeatDTOs;
using ApiApplication.DTOs.ShowtimeDTOs;
using ApiApplication.DTOs.TicketDTOs;
using AutoMapper;

namespace ApiApplication.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map ShowtimeEntity to ShowtimeDTO and reverse
            CreateMap<ShowtimeEntity, ShowtimeDTO>()
                .ForMember(dest => dest.Movie, opt => opt.MapFrom(src => src.Movie))
                .ForMember(dest => dest.SessionDate, opt => opt.MapFrom(src => src.SessionDate))
                .ForMember(dest => dest.AuditoriumId, opt => opt.MapFrom(src => src.AuditoriumId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            // Map MovieEntity to MovieDTO and reverse
            CreateMap<MovieEntity, MovieDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ImdbId, opt => opt.MapFrom(src => src.ImdbId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Stars, opt => opt.MapFrom(src => src.Stars))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.ReleaseDate))
                .ReverseMap();

            // Map SeatEntity to SeatDTO and reverse
            CreateMap<SeatEntity, SeatDTO>().ReverseMap();

            // Map TicketEntity to TicketDTO and reverse
            CreateMap<TicketEntity, TicketDTO>()
                .ReverseMap();

            CreateMap<TicketSeatEntity, TicketSeatDTO>().ReverseMap();
        }
    }
}
