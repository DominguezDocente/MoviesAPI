using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class IpRestrictionDTO
    {
        public int Id { get; set; }

        [Required]
        public required string Ip { get; set; }
    }

    public class CreateIpRestrictionDTO
    {
        public int KeyId { get; set; }

        [Required]
        public required string Ip { get; set; }
    }

    public class UpdateIpRestrictionDTO
    {

        [Required]
        public required string Ip { get; set; }
    }
}
