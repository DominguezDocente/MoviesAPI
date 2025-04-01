using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Validations
{
    public class FileTypeValidation : ValidationAttribute
    {
        private readonly string[] _allowTypes;

        public FileTypeValidation(string[] allowTypes)
        {
            _allowTypes = allowTypes;
        }

        public FileTypeValidation(FileTypeGroup fileTypeGroup)
        {
            if (fileTypeGroup == FileTypeGroup.Image)
            {
                _allowTypes = ["image/jpg", "image/jpeg", "image/png", "image/gif"];
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            IFormFile formFile = value as IFormFile;

            if (formFile is null)
            {
                return ValidationResult.Success;
            }

            if (!_allowTypes.Contains(formFile.ContentType))
            {
                return new ValidationResult($"El tipo del archivo debe ser  uno de los siguientes {string.Join(",", _allowTypes)}");
            }

            return ValidationResult.Success;
        }
    }
}
