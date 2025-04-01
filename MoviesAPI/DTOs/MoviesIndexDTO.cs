namespace MoviesAPI.DTOs
{
    public class MoviesIndexDTO
    {
        public List<MovieDTO> FutureReleases { get; set; }
        public List<MovieDTO> InCinema { get; set; }

    }
}
