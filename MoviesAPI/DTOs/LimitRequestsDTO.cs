using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class LimitRequestsDTO
    {
        public const string SECTION = "LimitRequests";

        [Required]
        public int RequestsPerDayFree { get; set; }
    }
}
