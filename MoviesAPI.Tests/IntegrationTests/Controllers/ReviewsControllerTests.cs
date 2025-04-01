using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using Newtonsoft.Json;

namespace MoviesAPI.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class ReviewsControllerTests : BaseTests
    {
        private static readonly string url = "/api/Movies/1/Reviews";

        [TestMethod]
        public async Task GetReviewsReturns404NotExistanceMovie()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            WebApplicationFactory<StartUp> factory = BuildWebApplicationFactory(dbName);

            // Act
            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            Assert.AreEqual(StatusCodes.Status404NotFound, ((int)response.StatusCode));
        }

        [TestMethod]
        public async Task GetReviewsReturnsEmptyList()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            WebApplicationFactory<StartUp> factory = BuildWebApplicationFactory(dbName);

            ApplicationDbContext context = BuildContext(dbName);
            Movie movie = new Movie { Title = "Película 1" };
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            // Act
            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();

            List<ReviewDTO> reviews = JsonConvert.DeserializeObject<List<ReviewDTO>>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(0, reviews.Count);
        }
    }
}
