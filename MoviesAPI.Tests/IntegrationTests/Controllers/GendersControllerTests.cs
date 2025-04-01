using AutoMapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using Newtonsoft.Json;

namespace MoviesAPI.Tests.IntegrationTests.Controllers
{
    [TestClass]
    public class GendersControllerTests : BaseTests
    {
        private static readonly string url = "api/genders";

        [TestMethod]
        public async Task GetAllGendersListEmpty()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            WebApplicationFactory<StartUp> factory = BuildWebApplicationFactory(dbName);

            // Act
            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();

            List<GenderDTO>? genders = JsonConvert.DeserializeObject<List<GenderDTO>>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(0, genders.Count);
        }

        [TestMethod]
        public async Task GetAllGenders()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            WebApplicationFactory<StartUp> factory = BuildWebApplicationFactory(dbName);

            ApplicationDbContext context = BuildContext(dbName);
            await context.Genders.AddAsync(new Gender { Name = "Género 1" });
            await context.Genders.AddAsync(new Gender { Name = "Género 2" });
            await context.SaveChangesAsync();

            // Act
            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode();

            List<GenderDTO>? genders = JsonConvert.DeserializeObject<List<GenderDTO>>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(2, genders.Count);
        }

        [TestMethod]
        public async Task DeleteGender()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            WebApplicationFactory<StartUp> factory = BuildWebApplicationFactory(dbName);

            ApplicationDbContext context = BuildContext(dbName);
            Gender gender = new Gender { Name = "Género 1" };
            await context.Genders.AddAsync(gender);
            await context.SaveChangesAsync();

            // Act
            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.DeleteAsync($"{url}/{gender.Id}");

            // Assert
            response.EnsureSuccessStatusCode();

            ApplicationDbContext context2 = BuildContext(dbName);
            bool exists = await context2.Genders.AnyAsync();

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task DeleteGenderReturn401()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            WebApplicationFactory<StartUp> factory = BuildWebApplicationFactory(dbName, ignoreSecurity: false);

            // Act
            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.DeleteAsync($"{url}/1");

            // Assert
            Assert.AreEqual("Unauthorized", response.ReasonPhrase);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, ((int)response.StatusCode));
        }
    }
}
