using MoviesAPI.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Movie : IId
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; }

        public bool InCinema { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Poster { get; set; }

        public DateTime? DeletedAt { get; set; } = null;

        public List<MovieActor> MoviesActors { get; set; }

        public List<MovieGender> MoviesGenders { get; set; }

        public List<MovieCinemaRoom> MoviesCinemaRooms { get; set; }

    }
}
