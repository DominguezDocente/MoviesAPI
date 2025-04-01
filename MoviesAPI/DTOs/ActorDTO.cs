using MoviesAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class ActorDTO : PatchActorDTO
    {
        public int Id { get; set; }

        public string Photo { get; set; }
    }

    public class CreateActorDTO : PatchActorDTO
    {
        [FileWeightValidation(maxWeightInMegaBytes: 4)]
        [FileTypeValidation(fileTypeGroup: FileTypeGroup.Image)]
        public IFormFile Photo { get; set; }
    }

    public class PatchActorDTO
    {
        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        public DateTime Birthdate { get; set; }
    }
}
