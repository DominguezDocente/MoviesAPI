using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    public class IpRestrictionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUsersService _usersService;

        public IpRestrictionsController(ApplicationDbContext context, IUsersService usersService)
        {
            _context = context;
            _usersService = usersService;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreateIpRestrictionDTO dto)
        {
            APIKey key = await _context.APIKeys.FirstOrDefaultAsync(k => k.Id == dto.KeyId);

            if (key is null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            string userId = _usersService.GetUserId();

            if (key.UserId != userId)
            {
                return Forbid();
            }

            IPRestriction domainRestriction = new IPRestriction
            {
                APIKeyId = dto.KeyId,
                IP = dto.Ip,
            };

            _context.Add(domainRestriction);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, UpdateIpRestrictionDTO dto)
        {
            IPRestriction restriction = await _context.IPRestrictions.Include(r => r.APIKey)
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

            restriction.IP = dto.Ip;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            IPRestriction restriction = await _context.IPRestrictions.Include(r => r.APIKey)
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

            _context.IPRestrictions.Remove(restriction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
