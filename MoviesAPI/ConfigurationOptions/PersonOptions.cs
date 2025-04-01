using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.ConfigurationOptions
{
    public class PersonOptions
    {
        public const string SECTION = "AuthorSection";

        [Required]
        public required string Name { get; set; }

        [Required]
        public int Year { get; set; }
    }
}
