using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manruka.API.Data;
using Manruka.API.Entities;

namespace Manruka.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        // Endpoint ini dipanggil oleh Admin Booking Page untuk fitur Auto-fill
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Ambil semua user, urutkan berdasarkan nama agar rapi
            return await _context.Users
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
    }
}