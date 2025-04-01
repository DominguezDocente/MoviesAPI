using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class UserInfo
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
