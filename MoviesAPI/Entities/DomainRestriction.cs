using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class DomainRestriction
    {
        public int Id { get; set; }
        
        public int APIKeyId { get; set; }

        [Required]
        public required string Domain { get; set; }

        public APIKey APIKey { get; set; }
    }
}
