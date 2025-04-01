using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Entities
{
    public class Invoice
    {
        public int Id { get; set; }

        public required string UserId { get; set; }

        public User User { get; set; }

        public bool Paid { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public DateTime EmissionDate { get; set; }

        public DateTime LimitPaymentDate { get; set; }
    }
}
