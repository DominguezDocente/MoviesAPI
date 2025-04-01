using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class DomainRestrictionDTO
    {
        public int Id { get; set; }

        [Required]
        public required string Domain { get; set; }
    }

    public class CreateDomainRestrictionDTO
    {
        public int KeyId { get; set; }

        [Required]
        public required string Domain { get; set; }
    }

    public class UpdateDomainRestrictionDTO
    {

        [Required]
        public required string Domain { get; set; }
    }
}
