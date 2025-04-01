using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MoviesAPI.Controllers;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;

namespace MoviesAPI.Tests.UnitTests.Controllers
{
    [TestClass]
    public class MoviesControllerTests : BaseTests
    {
        private string CreateTestData()
        {
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);

            Movie movieWithGender = new Movie
            {
                Title = "Película CON Género",
                ReleaseDate = DateTime.Today.AddMonths(-1),
                InCinema = false
            };

            List<Movie> movies = new List<Movie>
            {
                new Movie
                {
                    Title = "Película estrenada",
                    ReleaseDate = DateTime.Today.AddMonths(-1),
                    InCinema = false
                },

                new Movie
                {
                    Title = "Película EN cines",
                    ReleaseDate = DateTime.Today.AddMonths(-1),
                    InCinema = true
                },

                new Movie
                {
                    Title = "Película NO estrenada",
                    ReleaseDate = DateTime.Today.AddMonths(1),
                    InCinema = false
                },
            };

            movies.Add(movieWithGender);

            Gender gender = new Gender { Name = "Género 1" };

            context.Genders.Add(gender);
            context.Movies.AddRange(movies);
            context.SaveChanges();

            MovieGender movieGender = new MovieGender
            {
                GenderId = gender.Id,
                MovieId = movieWithGender.Id
            };

            context.MoviesGenders.Add(movieGender);
            context.SaveChanges();

            return nameDb;
        }

        [TestMethod]
        public async Task FilterByTitle()
        {
            // Arrange
            string nameDb = CreateTestData();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Mock<ILogger<MoviesController>> loggerMock = new Mock<ILogger<MoviesController>>();

            MoviesController controller = new MoviesController(context, mapper, null, loggerMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            string movieTitle = "Película estrenada";
            MoviesFilterDTO dto = new MoviesFilterDTO
            {
                Title = movieTitle,
                RecordsPerPage = 10
            };

            ActionResult<List<MovieDTO>> response = await controller.Filter(dto);

            // Assert
            List<MovieDTO> movies = response.Value;
            Assert.AreEqual(1, movies.Count);
            Assert.AreEqual("Película estrenada", movies.First().Title);
        }

        [TestMethod]
        public async Task FilterByInCinema()
        {
            // Arrange
            string nameDb = CreateTestData();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Mock<ILogger<MoviesController>> loggerMock = new Mock<ILogger<MoviesController>>();

            MoviesController controller = new MoviesController(context, mapper, null, loggerMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            MoviesFilterDTO dto = new MoviesFilterDTO
            {
                InCinema = true,
                RecordsPerPage = 10
            };

            ActionResult<List<MovieDTO>> response = await controller.Filter(dto);

            // Assert
            List<MovieDTO> movies = response.Value;
            Assert.AreEqual(1, movies.Count);
            Assert.AreEqual("Película EN cines", movies.First().Title);
        }
        
        [TestMethod]
        public async Task FilterByNextReleases()
        {
            // Arrange
            string nameDb = CreateTestData();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Mock<ILogger<MoviesController>> loggerMock = new Mock<ILogger<MoviesController>>();

            MoviesController controller = new MoviesController(context, mapper, null, loggerMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            MoviesFilterDTO dto = new MoviesFilterDTO
            {
                NextReleases = true,
                RecordsPerPage = 10
            };

            ActionResult<List<MovieDTO>> response = await controller.Filter(dto);

            // Assert
            List<MovieDTO> movies = response.Value;
            Assert.AreEqual(1, movies.Count);
            Assert.AreEqual("Película NO estrenada", movies.First().Title);
        }

        [TestMethod]
        public async Task FilterByGender()
        {
            // Arrange
            string nameDb = CreateTestData();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Mock<ILogger<MoviesController>> loggerMock = new Mock<ILogger<MoviesController>>();

            MoviesController controller = new MoviesController(context, mapper, null, loggerMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            Gender gender = await context.Genders.FirstAsync();
            MoviesFilterDTO dto = new MoviesFilterDTO
            {
                GenderId = gender.Id,
                RecordsPerPage = 10
            };

            ActionResult<List<MovieDTO>> response = await controller.Filter(dto);

            // Assert
            List<MovieDTO> movies = response.Value;
            Assert.AreEqual(1, movies.Count);
            Assert.AreEqual("Película CON Género", movies.First().Title);
        }
       
        [TestMethod]
        public async Task FilterOrderByTitleAscending()
        {
            // Arrange
            string nameDb = CreateTestData();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Mock<ILogger<MoviesController>> loggerMock = new Mock<ILogger<MoviesController>>();

            MoviesController controller = new MoviesController(context, mapper, null, loggerMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            MoviesFilterDTO dto = new MoviesFilterDTO
            {
                OrderByField = "title",
                OrderByAscending = true,
                RecordsPerPage = 10
            };

            ActionResult<List<MovieDTO>> response = await controller.Filter(dto);

            // Assert
            List<MovieDTO> movies = response.Value;

            ApplicationDbContext context2 = BuildContext(nameDb);
            List<Movie> moviesDb = await context2.Movies.OrderBy(m => m.Title).ToListAsync();

            Assert.AreEqual(moviesDb.Count, movies.Count);

            for(int i = 0; i < moviesDb.Count; i++)
            {
                Movie dbMovie = moviesDb[i];
                MovieDTO controllerMovie = movies[i];

                //Assert.AreEqual(dbMovie.Title, controllerMovie.Title);
                Assert.AreEqual(dbMovie.Id, controllerMovie.Id);
            }
        }

        [TestMethod]
        public async Task FilterOrderByTitleDescending()
        {
            // Arrange
            string nameDb = CreateTestData();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Mock<ILogger<MoviesController>> loggerMock = new Mock<ILogger<MoviesController>>();

            MoviesController controller = new MoviesController(context, mapper, null, loggerMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            MoviesFilterDTO dto = new MoviesFilterDTO
            {
                OrderByField = "title",
                OrderByAscending = false,
                RecordsPerPage = 10
            };

            ActionResult<List<MovieDTO>> response = await controller.Filter(dto);

            // Assert
            List<MovieDTO> movies = response.Value;

            ApplicationDbContext context2 = BuildContext(nameDb);
            List<Movie> moviesDb = await context2.Movies.OrderByDescending(m => m.Title).ToListAsync();

            Assert.AreEqual(moviesDb.Count, movies.Count);

            for (int i = 0; i < moviesDb.Count; i++)
            {
                Movie dbMovie = moviesDb[i];
                MovieDTO controllerMovie = movies[i];

                //Assert.AreEqual(dbMovie.Title, controllerMovie.Title);
                Assert.AreEqual(dbMovie.Id, controllerMovie.Id);
            }
        }

        [TestMethod]
        public async Task FilterByWrongFiledReturnsMovies()
        {
            // Arrange
            string nameDb = CreateTestData();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Mock<ILogger<MoviesController>> loggerMock = new Mock<ILogger<MoviesController>>();

            MoviesController controller = new MoviesController(context, mapper, null, loggerMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            MoviesFilterDTO dto = new MoviesFilterDTO
            {
                OrderByField = "noRealField",
                OrderByAscending = true,
                RecordsPerPage = 10
            };

            ActionResult<List<MovieDTO>> response = await controller.Filter(dto);

            // Assert
            List<MovieDTO> movies = response.Value;

            ApplicationDbContext context2 = BuildContext(nameDb);
            List<Movie> moviesDb = await context2.Movies.ToListAsync();

            Assert.AreEqual(moviesDb.Count, movies.Count);
            Assert.AreEqual(1, loggerMock.Invocations.Count);
        }
    }
}
