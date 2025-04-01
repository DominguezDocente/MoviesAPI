using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Gender : IId
    {
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        public List<MovieGender> MoviesGenders { get; set; }
    }
}
