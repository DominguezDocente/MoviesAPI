using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class GenderDTO : CreateGenderDTO
    {
        public int Id { get; set; }
    }

    public class CreateGenderDTO
    {
        [Required]
        [StringLength(64)]
        public string Name { get; set; }
    }
}
