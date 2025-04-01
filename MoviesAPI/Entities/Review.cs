using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Review : IId
    {
        public int Id { get; set; }

        public string Comment { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        public int MovieId { get; set; }

        public Movie Movie { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }
    }
}
