using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Helpers
{
    public class StartWithUpperCaseAttribute : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string valueString = value.ToString();
            valueString = valueString.Trim();
            string firstLetter = valueString[0].ToString();

            if (firstLetter != firstLetter.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayúscula.");
            }

            return ValidationResult.Success;

        }

    }
}
