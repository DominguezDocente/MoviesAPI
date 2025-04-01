using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Entities
{
    [PrimaryKey("Month", "Year")]
    public class IssuedInvoice
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
