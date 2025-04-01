using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Validations
    {
        [Required]
        [StringLength(10)]
        public string Name { get; set; }

        [Range(18, 120)]
        public int Age { get; set; }

        [CreditCard]
        public string CreditCard { get; set; }

        [Url]
        public string Url { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
