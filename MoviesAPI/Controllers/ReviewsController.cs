using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using System.Security.Claims;

namespace MoviesAPI.Controllers
{
    [Route("api/Movies/{movieId:int}/reviews")]
    [ApiController]
    [ServiceFilter(typeof(MovieExistsAttribute))]
    public class ReviewsController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReviewsController(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReviewDTO>>> Get(int movieId, [FromQuery] PaginationDTO dto)
        {
            IQueryable<Review> queryable = _context.Reviews.Include(r => r.User).AsQueryable();
            queryable = queryable.Where(r => r.MovieId == movieId);
            return await GetPagination<Review, ReviewDTO>(dto, queryable);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int movieId, [FromBody] CreateReviewDTO dto)
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier).Value;

            bool reviewExists = await _context.Reviews.AnyAsync(r => r.MovieId == movieId && r.UserId == userId);

            if (reviewExists)
            {
                return BadRequest("El ususario ya ha escrito una reseña para la película");
            }

            Review review = _mapper.Map<Review>(dto);
            review.MovieId = movieId;
            review.UserId = userId;

            await _context.AddAsync(review);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPut("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(int movieId, int reviewId, [FromBody] CreateReviewDTO dto)
        {
            Review? review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
            {
                return NotFound();
            }

            string userId = HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier).Value;

            if (review.UserId != userId)
            {
                return Forbid();
            }

            review = _mapper.Map<Review>(dto);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int reviewId)
        {
            Review? review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review is null)
            {
                return NotFound();
            }

            string userId = HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier).Value;

            if (review.UserId != userId)
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
