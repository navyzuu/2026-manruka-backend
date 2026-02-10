using Manruka.API.Data;
using Manruka.API.DTOs;
using Manruka.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manruka.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/rooms (Semua orang bisa lihat)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
        {
            return await _context.Rooms
                .Select(r => new RoomDto 
                { 
                    Id = r.Id, 
                    Name = r.Name, 
                    Capacity = r.Capacity, 
                    Description = r.Description, 
                    IsAvailable = r.IsAvailable 
                })
                .ToListAsync();
        }

        // POST: api/rooms (Harusnya Admin only, tapi sementara kita buka dulu)
        [HttpPost]
        public async Task<ActionResult<Room>> CreateRoom(CreateRoomDto request)
        {
            var room = new Room
            {
                Name = request.Name,
                Capacity = request.Capacity,
                Description = request.Description,
                IsAvailable = true
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ruangan berhasil dibuat", roomId = room.Id });
        }
    }
}