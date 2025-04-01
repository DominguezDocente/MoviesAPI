using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Services;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [DisableLimitRequests]
    public class DomainRestrictionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUsersService _usersService;

        public DomainRestrictionsController(ApplicationDbContext context, IUsersService usersService)
        {
            _context = context;
            _usersService = usersService;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateDomainRestrictionDTO dto)
        {
            APIKey key = await _context.APIKeys.FirstOrDefaultAsync(k => k.Id == dto.KeyId);

            if (key is null)
            {
                //return NotFound();
                return StatusCode(StatusCodes.Status404NotFound);
            }

            string userId = _usersService.GetUserId();

            if (key.UserId != userId)
            {
                return Forbid();
            }

            DomainRestriction domainRestriction = new DomainRestriction
            {
                APIKeyId = dto.KeyId,
                Domain = dto.Domain,
            };

            _context.Add(domainRestriction);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, UpdateDomainRestrictionDTO dto)
        {
            DomainRestriction restriction = await _context.DomainRestriction.Include(r => r.APIKey)
                                                                            .FirstOrDefaultAsync(r => r.Id == id);

            if (restriction is null)
            {
                return NotFound();
            }

            string userId = _usersService.GetUserId();

            if (restriction.APIKey.UserId != userId)
            {
                return Forbid();
            }

            restriction.Domain = dto.Domain;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            DomainRestriction restriction = await _context.DomainRestriction.Include(r => r.APIKey)
                                                                            .FirstOrDefaultAsync(r => r.Id == id);

            if (restriction is null)
            {
                return NotFound();
            }

            string userId = _usersService.GetUserId();

            if (restriction.APIKey.UserId != userId)
            {
                return Forbid();
            }

            _context.DomainRestriction.Remove(restriction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
