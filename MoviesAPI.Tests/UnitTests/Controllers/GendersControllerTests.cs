using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class GendersControllerTests : BaseTests
    {
        [TestMethod]
        public async Task Get_ReturnAllGenders()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            await context.Genders.AddAsync(new Gender { Name = "Género 1" });
            await context.Genders.AddAsync(new Gender { Name = "Género 2" });
            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(nameDb); // se crea otro contexto para probar sin que este contanimado ya que en el anterior
                                                                  // contexto se agregaron por defecto

            // Act
            GendersController controller = new GendersController(context2, mapper, null);
            ActionResult<List<GenderDTO>> response = await controller.Get();

            // Assert
            List<GenderDTO>? genders = response.Value;
            Assert.AreEqual(2, genders.Count);
        }

        [TestMethod]
        public async Task GetGenderByNotExistentId()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            // Act
            GendersController controller = new GendersController(context, mapper);
            ActionResult<GenderDTO> response = await controller.Get(1);

            // Assert
            StatusCodeResult result = response.Result as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [TestMethod]
        public async Task GetGenderById()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Gender gender = new Gender { Name = "Género 1" };
            await context.Genders.AddAsync(gender);
            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(nameDb);

            // Act
            GendersController controller = new GendersController(context2, mapper);
            ActionResult<GenderDTO> response = await controller.Get(gender.Id);

            // Assert
            GenderDTO dto = response.Value;
            Assert.AreEqual(gender.Id, dto.Id);
        }

        [TestMethod]
        public async Task PostGender()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            CreateGenderDTO newGender = new CreateGenderDTO { Name = "Género 1" };

            // Act
            GendersController controller = new GendersController(context, mapper);
            ActionResult response = await controller.Post(newGender);

            // Assert
            CreatedAtRouteResult result = response as CreatedAtRouteResult;
            Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);

            ApplicationDbContext context2 = BuildContext(nameDb);
            int quantity = await context2.Genders.CountAsync();
            Assert.AreEqual(1, quantity);
        }

        [TestMethod]
        public async Task PutGender()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Gender gender = new Gender { Name = "Género 1" };
            await context.Genders.AddAsync(gender);
            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(nameDb);
            ApplicationDbContext context3 = BuildContext(nameDb);

            CreateGenderDTO dto = new CreateGenderDTO { Name = "EDITED" };

            // Act
            GendersController controller = new GendersController(context2, mapper);
            ActionResult response = await controller.Put(gender.Id, dto);

            // Assert
            StatusCodeResult result = response as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);

            Gender gender2 = await context3.Genders.FirstOrDefaultAsync(g => g.Id == gender.Id);
            Assert.AreEqual(gender2.Name, "EDITED");
        }

        [TestMethod]
        public async Task DeleteGenderByNotExistentId()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            // Act
            GendersController controller = new GendersController(context, mapper);
            ActionResult<GenderDTO> response = await controller.Delete(1);

            // Assert
            StatusCodeResult result = response.Result as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [TestMethod]
        public async Task DeleteGendertId()
        {
            // Arrange
            string nameDb = Guid.NewGuid().ToString();
            ApplicationDbContext context = BuildContext(nameDb);
            IMapper mapper = ConfigureAutoMapper();

            Gender gender = new Gender { Name = "Género 1" };
            await context.Genders.AddAsync(gender);
            await context.SaveChangesAsync();

            ApplicationDbContext context2 = BuildContext(nameDb);
            ApplicationDbContext context3 = BuildContext(nameDb);

            // Act
            GendersController controller = new GendersController(context2, mapper);
            ActionResult response = await controller.Delete(gender.Id);

            // Assert
            StatusCodeResult result = response as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);

            Gender? gender2 = await context3.Genders.FirstOrDefaultAsync(g => g.Id == gender.Id);
            bool notExists = gender2 is null;
            Assert.IsTrue(notExists);
        }
    }
}
