using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Helpers;
using MoviesAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class MovieDTO : PatchMovieDTO
    {
        public int Id { get; set; }

        public string Poster { get; set; }
    }

    public class CreateMovieDTO : PatchMovieDTO
    {

        [FileWeightValidation(maxWeightInMegaBytes: 4)]
        [FileTypeValidation(fileTypeGroup: FileTypeGroup.Image)]
        public IFormFile Poster { get; set; }

        [ModelBinder(BinderType = typeof(CustomTypeBinder<List<int>>))]
        public List<int> GendersIDs { get; set; }

        [ModelBinder(BinderType = typeof(CustomTypeBinder<List<CreateMovieActorDTO>>))]
        public List<CreateMovieActorDTO> Actors { get; set; }
    }

    public class PatchMovieDTO
    {
        [Required]
        [StringLength(256)]
        public string Title { get; set; }

        public bool InCinema { get; set; }

        public DateTime ReleaseDate { get; set; }

    }
}
