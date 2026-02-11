using AutoMapper;
using server.DTOs;
using server.Models;

namespace server.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        CreateMap<Movie, MovieDto>();
        CreateMap<Movie, MovieDetailDto>();
        CreateMap<CreateMovieDto, Movie>();

        CreateMap<Showtime, ShowtimeDto>()
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie.Title));

        CreateMap<Booking, BookingResponseDto>()
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Showtime.Movie.Title))
            .ForMember(dest => dest.Theater, opt => opt.MapFrom(src => src.Showtime.Theater))
            .ForMember(dest => dest.ShowTime, opt => opt.MapFrom(src => src.Showtime.StartTime))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
