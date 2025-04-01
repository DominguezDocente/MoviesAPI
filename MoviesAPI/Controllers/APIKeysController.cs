using AutoMapper;
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
    [Route("api/APIKeys")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [DisableLimitRequests]
    public class APIKeysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IKeysService _keysService;
        private readonly IUsersService _usersService;

        public APIKeysController(ApplicationDbContext context, IMapper mapper, IKeysService keysService, IUsersService usersService)
        {
            _context = context;
            _mapper = mapper;
            _keysService = keysService;
            _usersService = usersService;
        }

        [HttpGet]
        public async Task<IEnumerable<KeyDTO>> Get()
        {
            string userId = _usersService.GetUserId();
            List<APIKey> keys = await _context.APIKeys.Include(k => k.DomainRestrictions)
                                                      .Include(k => k.IPRestrictions)
                                                      .Where(k => k.UserId == userId)
                                                      .ToListAsync();

            return _mapper.Map<IEnumerable<KeyDTO>>(keys);
        }

        [HttpGet("{id:int}", Name = "GetKey")]
        public async Task<ActionResult<KeyDTO>> Get(int id)
        {
            string userId = _usersService.GetUserId();
            APIKey key = await _context.APIKeys.Include(k => k.DomainRestrictions)
                                               .Include(k => k.IPRestrictions)
                                               .FirstOrDefaultAsync(k => k.Id == id);

            if (key is null)
            {
                return NotFound();
            }

            if (key.UserId != userId)
            {
                return Forbid();
            }

            return Ok(_mapper.Map<KeyDTO>(key));
        }

        [HttpPost]
        public async Task<ActionResult<KeyDTO>> Post(CreateKeyDTO dto)
        {
            string userId = _usersService.GetUserId();

            if (dto.KeyType == KeyType.Free)
            {
                bool userAlreadyHasFreeKey = await _context.APIKeys.AnyAsync(k => k.UserId == userId && k.KeyType == KeyType.Free);

                if (userAlreadyHasFreeKey)
                {
                    ModelState.AddModelError(nameof(dto.KeyType), "El usuario ya tiene una llave gratuita");
                    return ValidationProblem();
                }
            }

            APIKey model = await _keysService.CreateKey(userId, dto.KeyType);
            KeyDTO keyDTO = _mapper.Map<KeyDTO>(model);

            return CreatedAtRoute("GetKey", new { id = model.Id }, keyDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<KeyDTO>> Put(int id, UpdateKeyDTO dto)
        {
            string userId = _usersService.GetUserId();

            APIKey? apiKey = await _context.APIKeys.FirstOrDefaultAsync(k => k.Id == id);

            if (apiKey is null)
            {
                return NotFound();
            }

            if (userId != apiKey.UserId)
            {
                return Forbid();
            }

            if (dto.UpdateKey)
            {
                apiKey.Key = _keysService.GenerateKey();
            }

            apiKey.Active = dto.Active;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<KeyDTO>> Delete(int id)
        {
            string userId = _usersService.GetUserId();
            APIKey key = await _context.APIKeys.FirstOrDefaultAsync(k => k.Id == id);

            if (key is null)
            {
                return NotFound();
            }

            if (key.UserId != userId)
            {
                return Forbid();
            }

            if (key.KeyType == KeyType.Free)
            {
                ModelState.AddModelError("", "No puede eliminar su llave gratiuta");
                return ValidationProblem();
            }

            _context.APIKeys.Remove(key);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
