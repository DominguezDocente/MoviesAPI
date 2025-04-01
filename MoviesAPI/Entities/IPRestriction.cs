using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class IPRestriction
    {
        public int Id { get; set; }

        public int APIKeyId { get; set; }

        [Required]
        public required string IP { get; set; }

        public APIKey APIKey { get; set; }
    }
}
