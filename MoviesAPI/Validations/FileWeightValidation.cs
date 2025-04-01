using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Validations
{
    public class FileWeightValidation : ValidationAttribute
    {
        private readonly int _maxWeightInMegaBytes;

        public FileWeightValidation(int maxWeightInMegaBytes)
        {
            _maxWeightInMegaBytes = maxWeightInMegaBytes;
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

            if (formFile.Length > _maxWeightInMegaBytes * 1024 * 1024)
            {
                return new ValidationResult($"El peso del archivo no debe ser mayor a {_maxWeightInMegaBytes}Mb");
            }

            return ValidationResult.Success;
        }
    }
}
