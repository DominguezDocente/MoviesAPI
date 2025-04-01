using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Filters;
using MoviesAPI.Helpers;
using MoviesAPI.Jobs;
using MoviesAPI.Middlewares;
using MoviesAPI.Services;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Serilog;
using System.Text;

namespace MoviesAPI
{
    public class StartUp
    {
        public StartUp(ConfigurationManager configuration)
        {
            Configuration = configuration;
        }

        public ConfigurationManager Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("MyConnection"),
                    sqlServerOptions => sqlServerOptions.UseNetTopologySuite());

            });

            services.AddScoped<MovieExistsAttribute>();

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ErrorsFilter));
                options.Filters.Add(typeof(ExcecutionTimeFilter));
            })
            .AddNewtonsoftJson();

            // IAM
            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => 
                        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = false,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                            ClockSkew = TimeSpan.Zero
                        }
                    );

            //services.AddEndpointsApiExplorer();

            services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326)); // Indica el sistema de coordenadas para el planeta tierra

            services.AddSingleton(provider =>
            
                new MapperConfiguration(config => 
                {
                    GeometryFactory geometryFactory = provider.GetRequiredService<GeometryFactory>();
                    config.AddProfile(new AutoMapperProfiles(geometryFactory));
                }).CreateMapper()
            );

            services.AddAutoMapper(typeof(StartUp));

            services.AddHttpContextAccessor();

            services.AddTransient<IStorageService, LocalStorageService>();
            services.AddTransient<IUsersService, UsersService>();
            services.AddTransient<IKeysService, KeysService>();
            services.AddTransient<IHashService, HashService>();

            services.AddHostedService<InvoicesBackgroundService>();
        }

        public void Configure(WebApplication app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Permite servir recursos estáticos del wwwroot (por defecto)
            app.UseStaticFiles();

            app.UseCors();

            // Caché
            app.UseOutputCache();

            app.UseRouting();

            app.UseAuthorization();

            // Middlewares
            //app.Use(async (httpContext, next) =>
            //{
            //    ILogger<StartUp> logger = httpContext.RequestServices.GetRequiredService<ILogger<StartUp>>();

            //    logger.LogInformation($"Petición: {httpContext.Request.Method} {httpContext.Request.Path}");

            //    await next.Invoke();

            //    logger.LogInformation($"Respuesta: {httpContext.Response.StatusCode}");
            //});

            //app.Use(async (httpContext, next) =>
            //{
            //    if (httpContext.Request.Path == "/forbbiden-path")
            //    {
            //        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            //        await httpContext.Response.WriteAsync("Acceso denegado");
            //    }
            //    else
            //    {
            //        await next.Invoke();
            //    }
            //});

            app.UseLogRequestMiddleware();
            app.UseForbbidenPathMiddleware();
            app.UseLimitRequestsMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}