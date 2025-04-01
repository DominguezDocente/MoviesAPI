using Microsoft.AspNetCore.Identity;

namespace MoviesAPI.Entities
{   
    public class User : IdentityUser
    {
        public DateTime Birthdate { get; set; }
        public bool DefaultingDebtor { get; set; }
    }
}
