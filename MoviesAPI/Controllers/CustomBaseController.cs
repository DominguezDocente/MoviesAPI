using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Services;

namespace MoviesAPI.Controllers
{
    public class CustomBaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoviesController> _logger;
        private readonly IMapper _mapper;

        public CustomBaseController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        protected async Task<List<TDTO>> Get<TEntity, TDTO>() where TEntity : class
        {
            List<TEntity> entities = await _context.Set<TEntity>()
                                                   .AsNoTracking()
                                                   .ToListAsync();

            List<TDTO> dtos = _mapper.Map<List<TDTO>>(entities);
            return dtos;
        }

        protected async Task<List<TDTO>> GetPagination<TEntity, TDTO>(PaginationDTO dto, IQueryable<TEntity> queryable) where TEntity : class
        {
            await HttpContext.InsertPaginationParameters(queryable, dto.RecordsPerPage);
            List<TEntity> entities = await queryable.Paginate(dto).ToListAsync();
            return _mapper.Map<List<TDTO>>(entities);
        }

        protected async Task<ActionResult<TDTO>> Get<TEntity, TDTO>(int id) where TEntity : class, IId
        {
            TEntity entity = await _context.Set<TEntity>()
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
            {
                return NotFound();
            }

            TDTO dtos = _mapper.Map<TDTO>(entity);
            return dtos;
        }

        protected async Task<ActionResult> Post<TCreateDTO, TEntity, TReadDTO>(TCreateDTO dto, string routeName) where TEntity : class, IId
        {
            TEntity entity = _mapper.Map<TEntity>(dto);

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            TReadDTO readDTO = _mapper.Map<TReadDTO>(entity);

            return new CreatedAtRouteResult(routeName, new { id = entity.Id }, readDTO);
        }

        protected async Task<ActionResult> Put<TCreateDTO, TEntity>(TCreateDTO dto, int id) where TEntity : class, IId
        {
            TEntity entity = _mapper.Map<TEntity>(dto);
            entity.Id = id;

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<ActionResult> Delete<TEntity>(int id) where TEntity : class, IId
        {
            TEntity? entity = await _context.Set<TEntity>()
                                            .FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
            {
                return NotFound();
            }

            _context.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<List<TDTO>> GetPagination<TEntity, TDTO>(PaginationDTO dto) where TEntity : class
        {
            IQueryable<TEntity> queryable = _context.Set<TEntity>().AsQueryable();
            return await GetPagination<TEntity, TDTO>(dto, queryable);
        }

        protected async Task<ActionResult> Patch<TEntity, TDTO>(int id, JsonPatchDocument<TDTO> patchDocument) 
            where TDTO : class 
            where TEntity : class, IId
        {
            if (patchDocument is null)
            {
                return BadRequest();
            }

            TEntity? entity = await _context.Set<TEntity>().FirstOrDefaultAsync(a => a.Id == id);

            if (entity is null)
            {
                return NotFound();
            }

            TDTO dto = _mapper.Map<TDTO>(entity);

            patchDocument.ApplyTo(dto, ModelState);

            bool isValid = TryValidateModel(dto);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(dto, entity);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}

