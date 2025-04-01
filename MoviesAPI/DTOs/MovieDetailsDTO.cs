namespace MoviesAPI.DTOs
{
    public class MovieDetailsDTO : MovieDTO
    {
        public List<GenderDTO> Genders { get; set; }
        public List<MovieActorDetailsDTO> Actors { get; set; }
    }
}
