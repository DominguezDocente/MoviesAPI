using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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
    [DisableLimitRequests]
    public class ActorsController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        private readonly IStorageService _azureStorageService;
        private readonly string _container = "actors";

        public ActorsController(ApplicationDbContext context, IMapper mapper, IStorageService storageService) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _storageService = storageService;
            _azureStorageService = new AzureStorageService();
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginationDTO dto)
        {
            return await GetPagination<Actor, ActorDTO>(dto);
        }

        [HttpGet("{id}", Name = "getActor")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            return await Get<Actor, ActorDTO>(id);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] CreateActorDTO dto)
        {
            Actor entity = _mapper.Map<Actor>(dto);

            if (dto.Photo is not null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await dto.Photo.CopyToAsync(ms);
                    byte[] content = ms.ToArray();
                    string extension = Path.GetExtension(dto.Photo.FileName);
                    entity.Photo = await _storageService.SaveFileAsync(content, extension, _container, dto.Photo.ContentType);
                }
            }

            await _context.Actors.AddAsync(entity);
            await _context.SaveChangesAsync();

            ActorDTO actorDto = _mapper.Map<ActorDTO>(entity);

            return new CreatedAtRouteResult("getActor", new { id = entity.Id}, actorDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromForm] CreateActorDTO dto)
        {
            //Actor entity = _mapper.Map<Actor>(dto);
            //entity.Id = id;

            //_context.Entry(entity).State = EntityState.Modified;

            Actor? actorDB = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);

            if (actorDB is null)
            {
                return NotFound();
            }

            actorDB = _mapper.Map(dto, actorDB);

            if (dto.Photo is not null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await dto.Photo.CopyToAsync(ms);
                    byte[] content = ms.ToArray();
                    string extension = Path.GetExtension(dto.Photo.FileName);
                    actorDB.Photo = await _storageService.UpdateFile(content, 
                                                                     extension, 
                                                                     _container, 
                                                                     actorDB.Photo,
                                                                     dto.Photo.ContentType);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            return await Delete<Actor>(id);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PatchActorDTO> patchDocument)
        {
            //if (patchDocument is null)
            //{
            //    return BadRequest();
            //}

            //Actor? entity = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);

            //if (entity is null)
            //{
            //    return NotFound();
            //}

            //PatchActorDTO dto = _mapper.Map<ActorDTO>(entity);

            //patchDocument.ApplyTo(dto, ModelState);

            //bool isValid = TryValidateModel(dto);

            //if (!isValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //_mapper.Map(dto, entity);

            //await _context.SaveChangesAsync();

            //return NoContent();

            return await Patch<Actor, PatchActorDTO>(id, patchDocument);
        }
    }
}
