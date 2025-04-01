using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Services;
using System.Linq.Dynamic.Core;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [DisableLimitRequests]

    public class MoviesController : CustomBaseController
    {
        private readonly string _container = "movies";
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoviesController> _logger;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public MoviesController(ApplicationDbContext context, IMapper mapper, IStorageService storageService, ILogger<MoviesController> logger)
            :base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _storageService = storageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<MovieDTO>>> Get([FromQuery] PaginationDTO dto)
        {
            return await GetPagination<Movie, MovieDTO>(dto);
        }

        [HttpGet("top")]
        public async Task<ActionResult<MoviesIndexDTO>> Get()
        {
            int top = 5;
            DateTime today = DateTime.Today;

            List<Movie> nextReleases = await _context.Movies.Where(m => m.ReleaseDate > today)
                                                            .OrderBy(m => m.ReleaseDate)
                                                            .Take(top)
                                                            .ToListAsync();

            List<Movie> inCinema = await _context.Movies.Where(m => m.InCinema)
                                                        .Take(top)
                                                        .ToListAsync();

            MoviesIndexDTO result = new MoviesIndexDTO
            {
                FutureReleases = _mapper.Map<List<MovieDTO>>(nextReleases),
                InCinema = _mapper.Map<List<MovieDTO>>(inCinema)
            };

            return result;
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] MoviesFilterDTO dto)
        {
            IQueryable<Movie> queryable = _context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(dto.Title))
            {
                queryable = queryable.Where(m => m.Title.Contains(dto.Title));
            }

            if (dto.NextReleases)
            {
                queryable = queryable.Where(m => m.ReleaseDate > DateTime.Today)
                                                  .OrderBy(m => m.ReleaseDate);
            }

            if (dto.InCinema)
            {
                queryable = queryable.Where(m => m.InCinema);
            }

            if (dto.GenderId > 0)
            {
                queryable = queryable.Where(m => m.MoviesGenders.Select(mg => mg.GenderId)
                                                                .Contains(dto.GenderId));
            }

            if (!string.IsNullOrEmpty(dto.OrderByField))
            {
                string orderType = dto.OrderByAscending ? "ascending" : "descending";

                try
                {
                    queryable = queryable.OrderBy($"{dto.OrderByField} {orderType}");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            }

            await HttpContext.InsertPaginationParameters(queryable, dto.RecordsPerPage);

            List<Movie> entities = await queryable.Paginate(dto.Pagination).ToListAsync();

            return _mapper.Map<List<MovieDTO>>(entities);
        }

        [HttpGet("{id:int}", Name = "getMovie")]
        public async Task<ActionResult<MovieDetailsDTO>> Get(int id)
        {
            Movie entity = await _context.Movies.Include(m => m.MoviesActors)
                                                    .ThenInclude(ma => ma.Actor)
                                                .Include(m => m.MoviesGenders)
                                                    .ThenInclude(mg => mg.Gender)                
                                                .FirstOrDefaultAsync(a => a.Id == id);

            if (entity is null)
            {
                return NotFound();
            }

            return _mapper.Map<MovieDetailsDTO>(entity);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] CreateMovieDTO dto)
        {
            Movie entity = _mapper.Map<Movie>(dto);

            if (dto.Poster is not null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await dto.Poster.CopyToAsync(ms);
                    byte[] content = ms.ToArray();
                    string extension = Path.GetExtension(dto.Poster.FileName);
                    entity.Poster = await _storageService.SaveFileAsync(content, extension, _container, dto.Poster.ContentType);
                }
            }

            AssingOrderToActor(entity);
            await _context.Movies.AddAsync(entity);
            await _context.SaveChangesAsync();

            MovieDTO movieDto = _mapper.Map<MovieDTO>(entity);

            return new CreatedAtRouteResult("getMovie", new { id = entity.Id }, movieDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromForm] CreateMovieDTO dto)
        {
            Movie? movieDb = await _context.Movies.Include(m => m.MoviesActors)
                                                  .Include(m => m.MoviesGenders)
                                                  .FirstOrDefaultAsync(a => a.Id == id);

            if (movieDb is null)
            {
                return NotFound();
            }

            movieDb = _mapper.Map(dto, movieDb);

            if (dto.Poster is not null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await dto.Poster.CopyToAsync(ms);
                    byte[] content = ms.ToArray();
                    string extension = Path.GetExtension(dto.Poster.FileName);
                    movieDb.Poster = await _storageService.UpdateFile(content,
                                                                     extension,
                                                                     _container,
                                                                     movieDb.Poster,
                                                                     dto.Poster.ContentType);
                }
            }

            AssingOrderToActor(movieDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            Movie? entity = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
            {
                return NotFound();
            }

            entity.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PatchMovieDTO> patchDocument)
        {
            return await Patch<Movie, PatchMovieDTO>(id, patchDocument);
        }

        private void AssingOrderToActor(Movie movie)
        {
            if (movie.MoviesActors is  not null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }
    }
}
