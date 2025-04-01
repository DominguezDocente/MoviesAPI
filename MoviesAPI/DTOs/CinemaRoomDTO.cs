using MoviesAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class CinemaRoomDTO : CreateCinemaRoomDTO
    {
        public int Id { get; set; }
    }

    public class CreateCinemaRoomDTO
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
    }

    public class CinemaRoomNearbyFilterDTO
    {

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        public double DistanceInKm 
        { 
            get => _distanceInKm; 
            set => _distanceInKm = value > _maxDistanceKm ? _maxDistanceKm : value; 
        }

        private double _distanceInKm = 10;
        private double _maxDistanceKm = 50;
    }

    public class CinemaRoomNearbyDTO : CinemaRoomDTO
    {
        public double DistanceInMeters { get; set; }
    }
}
