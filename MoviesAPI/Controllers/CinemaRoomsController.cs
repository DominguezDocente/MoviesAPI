using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CinemaRoomsController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;

        public CinemaRoomsController(ApplicationDbContext context, IMapper mapper, GeometryFactory geometryFactory) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _geometryFactory = geometryFactory;
        }

        [HttpGet]
        public async Task<ActionResult<List<CinemaRoomDTO>>> Get()
        {
            return await Get<CinemaRoom, CinemaRoomDTO>();
        }

        [HttpGet("{id:int}", Name = "getCinemaRoom")]
        public async Task<ActionResult<CinemaRoomDTO>> Get([FromRoute] int id)
        {
            return await Get<CinemaRoom, CinemaRoomDTO>(id);
        }

        [HttpGet("Nearby")]
        public async Task<ActionResult<List<CinemaRoomNearbyDTO>>> GetNearby([FromQuery] CinemaRoomNearbyFilterDTO filterDto)
        {
            Point userLocation = _geometryFactory.CreatePoint(new Coordinate(filterDto.Longitude, filterDto.Latitude));

            List<CinemaRoom> list = await _context.CinemaRooms.ToListAsync();

            List<CinemaRoomNearbyDTO> cinemaRooms = await _context.CinemaRooms.OrderBy(x => x.Location.Distance(userLocation))
                                                                              .Where(x => x.Location.IsWithinDistance(userLocation, filterDto.DistanceInKm * 1000))
                                                                              .Select(x => new CinemaRoomNearbyDTO
                                                                              {
                                                                                  Id = x.Id,
                                                                                  Name = x.Name,
                                                                                  Latitude = x.Location.Y,
                                                                                  Longitude = x.Location.X,
                                                                                  DistanceInMeters = Math.Round(x.Location.Distance(userLocation))
                                                                              }).ToListAsync();

            return cinemaRooms;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateCinemaRoomDTO dto)
        {
            return await Post<CreateCinemaRoomDTO, CinemaRoom, CinemaRoomDTO>(dto, "getCinemaRoom");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] CreateCinemaRoomDTO dto)
        {
            return await Put<CreateCinemaRoomDTO, CinemaRoom>(dto, id);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            return await Delete<CinemaRoom>(id);
        }
    }
}
