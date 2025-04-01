using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoviesAPI;
using MoviesAPI.ConfigurationOptions;
using MoviesAPI.DTOs;
using MoviesAPI.Filters;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Dictionary<string, string> configDictionary = new Dictionary<string, string>
{
    { "WhoAmI", "Un Diccionario en memoria" }
};

builder.Configuration.AddInMemoryCollection(configDictionary);

StartUp startUp = new StartUp(builder.Configuration);

startUp.ConfigureServices(builder.Services);

// Configurar Serilog para guardar logs en un archivo
//Log.Logger = new LoggerConfiguration()
//    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();


builder.Services.AddOptions<LimitRequestsDTO>()
                .Bind(builder.Configuration
                .GetSection(LimitRequestsDTO.SECTION))
                .ValidateDataAnnotations()
                .ValidateOnStart();

builder.Services.AddOptions<PersonOptions>()
                .Bind(builder.Configuration
                .GetSection(PersonOptions.SECTION))
                .ValidateDataAnnotations()
                .ValidateOnStart();

builder.Services.AddOptions<RatesOptions>()
                .Bind(builder.Configuration
                .GetSection(RatesOptions.SECTION))
                .ValidateDataAnnotations()
                .ValidateOnStart();

builder.Services.AddSingleton<RatesPaymentProcess>();

builder.Services.AddDataProtection();

string[] origenesPermitidos = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!;

builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCORS =>
    {
        opcionesCORS.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("cantidad-total-registros");
    });
});

// Para encriptacion IDataProtector
builder.Services.AddDataProtection();

// caché
builder.Services.AddOutputCache(options => 
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

// filters
builder.Services.AddScoped<MyActionFilter>();

WebApplication app = builder.Build();

startUp.Configure(app, app.Environment);

app.Run();
