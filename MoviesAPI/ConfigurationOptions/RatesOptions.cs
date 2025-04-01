using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.ConfigurationOptions
{
    public class RatesOptions
    {
        public const string SECTION = "Rates";

        [Required]
        public required decimal Day { get; set; }

        [Required]
        public required decimal Night { get; set; }
    }
}
