using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Security.Claims;

namespace MoviesAPI
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        //public ApplicationDbContext(DbContextOptions options) : base(options)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Actor> Actors { get; set; }
        public DbSet<APIKey> APIKeys { get; set; }
        public DbSet<APIRequest> APIRequests { get; set; }
        public DbSet<CinemaRoom> CinemaRooms { get; set; }
        public DbSet<DomainRestriction> DomainRestriction { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<IPRestriction> IPRestrictions { get; set; }
        public DbSet<IssuedInvoice> IssuedInvoices { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieActor> MoviesActors { get; set; }
        public DbSet<MovieCinemaRoom> MoviesCinemaRooms { get; set; }
        public DbSet<MovieGender> MoviesGenders { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieActor>()
                        .HasKey(ma => new { ma.ActorId, ma.MovieId });

            modelBuilder.Entity<MovieGender>()
                        .HasKey(ma => new { ma.GenderId, ma.MovieId });

            modelBuilder.Entity<MovieCinemaRoom>()
                        .HasKey(mcr => new { mcr.MovieId, mcr.CinemaRoomId });

            SeedData(modelBuilder);

            //modelBuilder.Entity<User>().ToTable("AspNetUsers");

            modelBuilder.Entity<Movie>().HasQueryFilter(m => m.DeletedAt == null);

            base.OnModelCreating(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // ⭐ SEED DATA ⭐


            // IAM
            //string adminRoleId = "167d3610-d801-420e-a9d3-d4636af4087c";
            //string adminUserId = "24014fbc-eddc-45d0-a3ce-32ea3b7308dd";

            //IdentityRole adminRole = new IdentityRole
            //{
            //    Id = adminRoleId,
            //    Name = "Admin",
            //    NormalizedName = "Admin",
            //};

            //PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

            //User adminUser = new User
            //{
            //    Id = adminUserId,
            //    UserName = "manuel@yopmail.com",
            //    NormalizedUserName = "manuel@yopmail.com",
            //    Email = "manuel@yopmail.com",
            //    NormalizedEmail = "manuel@yopmail.com",
            //    PasswordHash = passwordHasher.HashPassword(null, "123456"),
            //    Birthdate = new DateTime(1996, 5, 3)
            //};

            //modelBuilder.Entity<IdentityRole>().HasData(adminRole);
            //modelBuilder.Entity<IdentityUser>().HasData(adminUser);
            //modelBuilder.Entity<IdentityUserClaim<string>>().HasData(new IdentityUserClaim<string>()
            //{
            //    Id = 1,
            //    ClaimType = ClaimTypes.Role,
            //    UserId = adminUserId,
            //    ClaimValue = "Admin"
            //});

            // Géneros
            var action = new Gender { Id = 1, Name = "Action" };
            var comedy = new Gender { Id = 2, Name = "Comedy" };
            var drama = new Gender { Id = 3, Name = "Drama" };

            modelBuilder.Entity<Gender>().HasData(action, comedy, drama);

            // Actores
            var actor1 = new Actor { Id = 1, Name = "Leonardo DiCaprio", Birthdate = new DateTime(1974, 11, 11), Photo = "leonardo.jpg" };
            var actor2 = new Actor { Id = 2, Name = "Scarlett Johansson", Birthdate = new DateTime(1984, 11, 22), Photo = "scarlett.jpg" };
            var actor3 = new Actor { Id = 3, Name = "Robert Downey Jr.", Birthdate = new DateTime(1965, 04, 04), Photo = "robert.jpg" };

            modelBuilder.Entity<Actor>().HasData(actor1, actor2, actor3);

            // Películas
            var movie1 = new Movie { Id = 1, Title = "Inception", ReleaseDate = new DateTime(2010, 07, 16), InCinema = false, Poster = "inception.jpg" };
            var movie2 = new Movie { Id = 2, Title = "Avengers: Endgame", ReleaseDate = new DateTime(2019, 04, 26), InCinema = true, Poster = "endgame.jpg" };
            var movie3 = new Movie { Id = 3, Title = "The Wolf of Wall Street", ReleaseDate = new DateTime(2013, 12, 25), InCinema = false, Poster = "wolf.jpg" };

            modelBuilder.Entity<Movie>().HasData(movie1, movie2, movie3);

            // Relación MoviesActors
            modelBuilder.Entity<MovieActor>().HasData(
                new MovieActor { MovieId = 1, ActorId = 1, Character = "Dom Cobb", Order = 1 },
                new MovieActor { MovieId = 2, ActorId = 2, Character = "Black Widow", Order = 1 },
                new MovieActor { MovieId = 2, ActorId = 3, Character = "Iron Man", Order = 2 },
                new MovieActor { MovieId = 3, ActorId = 1, Character = "Jordan Belfort", Order = 1 }
            );

            // Relación MoviesGenders
            modelBuilder.Entity<MovieGender>().HasData(
                new MovieGender { MovieId = 1, GenderId = 1 }, // Inception - Action
                new MovieGender { MovieId = 2, GenderId = 1 }, // Avengers - Action
                new MovieGender { MovieId = 2, GenderId = 2 }, // Avengers - Comedy
                new MovieGender { MovieId = 3, GenderId = 3 }  // Wolf of Wall Street - Drama
            );

            // Salas de Cine

            GeometryFactory geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            modelBuilder.Entity<CinemaRoom>().HasData(
                new CinemaRoom { Id = 5, Name = "Cine Colombia los Molinos", Location = geometryFactory.CreatePoint(new Coordinate(-75.60411563077764, 6.233287313258494)) },    
                new CinemaRoom { Id = 6, Name = "Viva envigado Mall", Location = geometryFactory.CreatePoint(new Coordinate(-75.59094136146099, 6.176432492851308)) },    
                new CinemaRoom { Id = 7, Name = "Village East Cinema", Location = geometryFactory.CreatePoint(new Coordinate(-73.98605334286276, 40.73121176677272)) }
            );
        }
    }
}
