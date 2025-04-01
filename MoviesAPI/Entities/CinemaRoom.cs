using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class CinemaRoom : IId
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        public Point Location { get; set; }

        public List<MovieCinemaRoom> MoviesCinemaRooms { get; set; }
    }
}
