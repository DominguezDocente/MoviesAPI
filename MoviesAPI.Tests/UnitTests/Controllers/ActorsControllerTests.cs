using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesAPI.Controllers;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Services;
using System.Text;

namespace MoviesAPI.Tests.UnitTests.Controllers
{
    [TestClass]
    public class ActorsControllerTests : BaseTests
    {

        [TestMethod]
        public async Task GetPagination()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            await context.Actors.AddRangeAsync(new List<Actor>
            {
                new Actor { Name = "Actor 1" },
                new Actor { Name = "Actor 2" },
                new Actor { Name = "Actor 3" },
            });

            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(nameDb);

            // Act
            ActorsController controller = new ActorsController(context2, mapper, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            ActionResult<List<ActorDTO>> page1 = await controller.Get(new PaginationDTO { Page = 1, RecordsPerPage = 2 });

            // Assert
            List<ActorDTO> page1Actors = page1.Value;
            Assert.AreEqual(2, page1Actors.Count);

            // Se reinicia para re generar los headers personalizados y trabjar con httpContext limpio (que simula la segunda petición a la siguiente página)
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            ActionResult<List<ActorDTO>> page2 = await controller.Get(new PaginationDTO { Page = 2, RecordsPerPage = 2 });

            List<ActorDTO> page2Actors = page2.Value;
            Assert.AreEqual(1, page2Actors.Count);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            ActionResult<List<ActorDTO>> page3 = await controller.Get(new PaginationDTO { Page = 3, RecordsPerPage = 2 });

            List<ActorDTO> page3Actors = page3.Value;
            Assert.AreEqual(0, page3Actors.Count);

        }

        [TestMethod]
        public async Task Create_ActorWithoutPhoto()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            CreateActorDTO dto = new CreateActorDTO
            {
                Name = "Alejo",
                Birthdate = DateTime.Now
            };

            Mock<IStorageService> storageServiceMock = new Mock<IStorageService>();
            storageServiceMock.Setup(x => x.SaveFileAsync(null, null, null, null))
                .Returns(Task.FromResult("url"));

            // Act
            ActorsController controller = new ActorsController(context, mapper, storageServiceMock.Object);
            ActionResult response = await controller.Post(dto);

            // Assert
            CreatedAtRouteResult? result = response as CreatedAtRouteResult;
            Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);

            ApplicationDbContext context2 = BuildContext(nameDb);
            List<Actor> list = await context2.Actors.ToListAsync();
            Assert.AreEqual(1, list.Count);
            Assert.IsNull(list.First().Photo);

            // Validar que el servicio de gruardar imagen no se llamo ya que el dto no tiene IFomrData para imagen
            Assert.AreEqual(0, storageServiceMock.Invocations.Count);
        }

        [TestMethod]
        public async Task Create_ActorWithPhoto()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            byte[] content = Encoding.UTF8.GetBytes("Imagen de prueba");
            FormFile file = new FormFile(new MemoryStream(content), 0, content.Length, "Data", "image.jpg");
            file.Headers = new HeaderDictionary();
            file.ContentType = "image/jpg";

            CreateActorDTO dto = new CreateActorDTO
            {
                Name = "Alejo",
                Birthdate = DateTime.Now,
                Photo = file
            };

            Mock<IStorageService> storageServiceMock = new Mock<IStorageService>();
            // Cuando el servicio reciba estos parametors debe simular retorno exitoso
            storageServiceMock.Setup(x => x.SaveFileAsync(content, ".jpg", "actors", file.ContentType))
                .Returns(Task.FromResult("url"));

            // Act
            ActorsController controller = new ActorsController(context, mapper, storageServiceMock.Object);
            ActionResult response = await controller.Post(dto);

            // Assert
            CreatedAtRouteResult? result = response as CreatedAtRouteResult;
            Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);

            ApplicationDbContext context2 = BuildContext(nameDb);
            List<Actor> list = await context2.Actors.ToListAsync();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("url", list.First().Photo);

            Assert.AreEqual(1, storageServiceMock.Invocations.Count);
        }

        [TestMethod]
        public async Task Patch_Returns404_IfActorDoesntExists()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Actor actor = new Actor { Name = "Actor 1" };
            await context.Actors.AddAsync(actor);
            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(nameDb);
            ActorsController controller = new ActorsController(context2, mapper, null);

            Mock<IObjectModelValidator> objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(x => x.Validate(It.IsAny<ActionContext>(),
                                                  It.IsAny<ValidationStateDictionary>(),
                                                  It.IsAny<string>(),
                                                  It.IsAny<object>()));

            controller.ObjectValidator = objectValidator.Object;

            JsonPatchDocument<PatchActorDTO> patchDoc = new JsonPatchDocument<PatchActorDTO>();

            // Act
            ActionResult response = await controller.Patch(2, patchDoc);

            // Assert
            StatusCodeResult? result = response as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [TestMethod]
        public async Task Patch_UpdateOnlyOneField()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            DateTime birthDate = DateTime.Now.AddMonths(-60);
            Actor actor = new Actor { Name = "Actor 1", Birthdate = birthDate };
            await context.Actors.AddAsync(actor);
            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(nameDb);
            ActorsController controller = new ActorsController(context2, mapper, null);

            Mock<IObjectModelValidator> objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(x => x.Validate(It.IsAny<ActionContext>(),
                                                  It.IsAny<ValidationStateDictionary>(),
                                                  It.IsAny<string>(),
                                                  It.IsAny<object>()));

            controller.ObjectValidator = objectValidator.Object;

            JsonPatchDocument<PatchActorDTO> patchDoc = new JsonPatchDocument<PatchActorDTO>();
            patchDoc.Operations.Add(new Operation<PatchActorDTO>("replace", "/name", null, "Ana"));

            // Act
            ActionResult response = await controller.Patch(1, patchDoc);

            // Assert
            StatusCodeResult? result = response as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);

            ApplicationDbContext context3 = BuildContext(nameDb);
            Actor actorDb = await context3.Actors.FirstAsync();
            Assert.AreEqual("Ana", actorDb.Name);
            Assert.AreEqual(birthDate, actorDb.Birthdate);
        }
    }
}
