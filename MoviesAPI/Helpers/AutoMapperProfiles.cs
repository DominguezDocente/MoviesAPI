using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace MoviesAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Gender, GenderDTO>().ReverseMap();
            CreateMap<CreateGenderDTO, Gender>();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<CreateActorDTO, Actor>()
                .ForMember(x => x.Photo, options => options.Ignore());
            CreateMap<PatchActorDTO, Actor>().ReverseMap();

            CreateMap<Movie, MovieDTO>().ReverseMap();
            CreateMap<CreateMovieDTO, Movie>()
                .ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.MoviesGenders, options => options.MapFrom(MapMoviesGenders))
                .ForMember(x => x.MoviesActors, options => options.MapFrom(MapMoviesAutors));

            CreateMap<PatchMovieDTO, Movie>().ReverseMap();
            CreateMap<Movie, MovieDetailsDTO>()
                .ForMember(x => x.Genders, options => options.MapFrom(MapMoviesGenders))
                .ForMember(x => x.Actors, options => options.MapFrom(MapMoviesActors));

            CreateMap<CinemaRoom, CinemaRoomDTO>()
                .ForMember(x => x.Latitude, x => x.MapFrom(y => y.Location.Y))
                .ForMember(x => x.Longitude, x => x.MapFrom(y => y.Location.X));

            CreateMap<CinemaRoomDTO, CinemaRoom>()
                .ForMember(x => x.Location, x => x.MapFrom(y => 
                geometryFactory.CreatePoint(new Coordinate(y.Longitude, y.Latitude))));

            CreateMap<CreateCinemaRoomDTO, CinemaRoom>()
                .ForMember(x => x.Location, x => x.MapFrom(y =>
                geometryFactory.CreatePoint(new Coordinate(y.Longitude, y.Latitude))));

            CreateMap<User, UserDTO>().ReverseMap();

            CreateMap<Review, ReviewDTO>()
                .ForMember(x => x.UserName, x => x.MapFrom(y => y.User.UserName));

            CreateMap<ReviewDTO, Review>();
            CreateMap<CreateReviewDTO, Review>();

            CreateMap<APIKey, KeyDTO>().ReverseMap();

            CreateMap<DomainRestriction, DomainRestrictionDTO>().ReverseMap();

            CreateMap<IPRestriction, IpRestrictionDTO>().ReverseMap();
        }

        private List<MovieActorDetailsDTO> MapMoviesActors(Movie movie, MovieDetailsDTO dto)
        {
            List<MovieActorDetailsDTO> result = new List<MovieActorDetailsDTO>();

            if (movie.MoviesActors is null)
            {
                return result;
            }

            foreach (MovieActor movieActor in movie.MoviesActors)
            {
                result.Add(new MovieActorDetailsDTO 
                { 
                    ActorId = movieActor.ActorId, 
                    Character = movieActor.Character,
                    ActorName = movieActor.Actor.Name
                });
            }

            return result;
        }

        private List<GenderDTO> MapMoviesGenders(Movie movie, MovieDetailsDTO dto)
        {
            List<GenderDTO> result = new List<GenderDTO>();

            if (movie.MoviesGenders is null)
            {
                return result;
            }

            foreach (MovieGender movieGender in movie.MoviesGenders)
            {
                result.Add(new GenderDTO { Id = movieGender.GenderId, Name = movieGender.Gender.Name });
            }

            return result;
        }

        private List<MovieGender> MapMoviesGenders(CreateMovieDTO dto, Movie movie)
        {
            List<MovieGender> result = new List<MovieGender>();

            if (dto.GendersIDs is null)
            {
                return result;
            }

            foreach (int id in dto.GendersIDs)
            {
                result.Add(new MovieGender { GenderId = id });
            }

            return result;
        }

        private List<MovieActor> MapMoviesAutors(CreateMovieDTO dto, Movie movie)
        {
            List<MovieActor> result = new List<MovieActor>();

            if (dto.Actors is null)
            {
                return result;
            }

            foreach (CreateMovieActorDTO actor in dto.Actors)
            {
                result.Add(new MovieActor { ActorId = actor.ActorId, Character = actor.Character });
            }

            return result;
        }
    }
}
