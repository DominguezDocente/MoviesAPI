using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Controllers;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesAPI.Tests.UnitTests.Controllers
{
    [TestClass]
    public class ReviewsControllerTests : BaseTests
    {
        [TestMethod]
        public async Task UserCannotCreateTowReviewsForTheSameMovie()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(dbName);
            IMapper mapper = ConfigureAutoMapper();

            CreateMovies(dbName);

            int movieId = await context.Movies.Select(m => m.Id).FirstAsync();
            Review review1 = new Review
            {
                MovieId = movieId,
                UserId = defaultUserId,
                Score = 5
            };

            await context.Reviews.AddAsync(review1);
            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(dbName);

            ReviewsController controller = new ReviewsController(context2, mapper);
            controller.ControllerContext = BuildControllerContext();

            // Act
            CreateReviewDTO dto = new CreateReviewDTO
            {
                Comment = "Muy buena",
                Score = 8,
            };

            ActionResult response = await controller.Post(movieId, dto);

            // Assert
            IStatusCodeActionResult result = response as IStatusCodeActionResult;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode.Value);
        }

        [TestMethod]
        public async Task CreateReview()
        {
            // Arrange
            string dbName = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(dbName);
            IMapper mapper = ConfigureAutoMapper();

            CreateMovies(dbName);

            ReviewsController controller = new ReviewsController(context, mapper);
            controller.ControllerContext = BuildControllerContext();

            // Act
            CreateReviewDTO dto = new CreateReviewDTO
            {
                Comment = "Muy buena",
                Score = 8,
            };

            int movieId = await context.Movies.Select(m => m.Id).FirstAsync();
            ActionResult response = await controller.Post(movieId, dto);

            // Assert
            CreatedResult result = response as CreatedResult;
            Assert.IsNotNull(result);

            ApplicationDbContext context2 = BuildContext(dbName);
            Review reviewDb = await context2.Reviews.FirstAsync();
            Assert.AreEqual(defaultUserId, reviewDb.UserId);
        }

        private void CreateMovies(string dbName)
        {
            ApplicationDbContext context = BuildContext(dbName);
            context.Movies.Add(new Movie { Title = "Película 1" });
            context.SaveChanges();
        }
    }
}
