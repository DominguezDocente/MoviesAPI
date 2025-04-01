using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Actor : IId
    {
        public int Id { get; set; }


        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        public DateTime Birthdate { get; set; }

        public string Photo { get; set; }

        public List<MovieActor> MoviesActors { get; set; }

    }
}
