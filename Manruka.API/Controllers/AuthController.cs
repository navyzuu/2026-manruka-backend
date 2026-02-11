using Manruka.API.Data;
using Manruka.API.DTOs;
using Manruka.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Manruka.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
            // Cek apakah NRP sudah ada
            if (await _context.Users.AnyAsync(u => u.NRP == request.NRP))
            {
                return BadRequest("NRP sudah terdaftar.");
            }

            var user = new User
            {
                Name = request.Name,
                NRP = request.NRP,
                Department = request.Department,
                Major = request.Major,
                YearEntry = request.YearEntry,
                Role = "User", // Default daftar sendiri = User
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registrasi berhasil" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.NRP == request.NRP);

            if (user == null)
            {
                return BadRequest("User tidak ditemukan.");
            }

            // --- LOGIKA HYBRID BARU ---
            bool isPasswordValid = false;

            // 1. Cek apakah password di DB sama persis dengan input (Untuk Admin Seed "admin123")
            if (user.PasswordHash == request.Password)
            {
                isPasswordValid = true;
            }
            // 2. Jika tidak sama, coba cek apakah itu Hash (Untuk User hasil Register)
            else
            {
                try
                {
                    // Cek hash hanya jika string terlihat seperti hash BCrypt (diawali $)
                    if (user.PasswordHash.StartsWith("$"))
                    {
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                    }
                }
                catch
                {
                    isPasswordValid = false;
                }
            }

            if (!isPasswordValid)
            {
                return BadRequest("Password salah.");
            }
            // --------------------------

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Role = user.Role
            });
        }
    }
}