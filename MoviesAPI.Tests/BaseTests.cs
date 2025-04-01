using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Helpers;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MoviesAPI.Tests
{
    public class BaseTests
    {
        protected string defaultUserId = "898fad99-d01e-43f6-b004-3055f1de6e3a"; 
        protected string defaultUserEmail = "test@test.com"; 

        protected ApplicationDbContext BuildContext(string dbName)
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName).Options;

            ApplicationDbContext dbContext = new ApplicationDbContext(options);

            return dbContext;
        }

        protected IMapper ConfigureAutoMapper()
        {
            MapperConfiguration config = new MapperConfiguration(options =>
            {
                GeometryFactory geomeryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                options.AddProfile(new AutoMapperProfiles(geomeryFactory));
            });

            return config.CreateMapper();
        }

        protected ControllerContext BuildControllerContext()
        {
            ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, defaultUserEmail),
                new Claim(ClaimTypes.Email, defaultUserEmail),
                new Claim(ClaimTypes.NameIdentifier, defaultUserId),
            }));

            return new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        protected WebApplicationFactory<StartUp> BuildWebApplicationFactory(string dbName, bool ignoreSecurity = true)
        {
            WebApplicationFactory<StartUp> factory = new WebApplicationFactory<StartUp>();

            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    ServiceDescriptor? descriptorDbContext = services.SingleOrDefault(d => 
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptorDbContext is not null)
                    {
                        services.Remove(descriptorDbContext);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase(dbName));

                    if (ignoreSecurity)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new FakeUserFilter());
                        });
                    }

                });
            });

            return factory;
        }
    }
}
