using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Filters;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AddHeaderFilter("Controlller", "Genders")]
    public class GendersController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string CACHE = "get-genders";

        public GendersController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        //[OutputCache(Tags = [CACHE])]
        [ServiceFilter<MyActionFilter>()]
        [AddHeaderFilter("Action", "Get-Genders")]
        public async Task<ActionResult<List<GenderDTO>>> Get()
        {
            return await Get<Gender, GenderDTO>();
        }

        [HttpGet("{id:int}", Name = "getGender")]
        [OutputCache(Tags = [CACHE])]
        public async Task<ActionResult<GenderDTO>> Get([FromRoute] int id)
        {
            return await Get<Gender, GenderDTO>(id);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateGenderDTO dto)
        {
            ActionResult response =  await Post<CreateGenderDTO, Gender, GenderDTO>(dto, "getGender");
            await _outputCacheStore.EvictByTagAsync(CACHE, default); // limpiar caché
            return response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] CreateGenderDTO dto)
        {
            ActionResult response  = await Put<CreateGenderDTO, Gender>(dto, id);
            await _outputCacheStore.EvictByTagAsync(CACHE, default);
            return response;
        }

        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            ActionResult response = await Delete<Gender>(id);
            await _outputCacheStore.EvictByTagAsync(CACHE, default);
            return response;
        }
    }
}
