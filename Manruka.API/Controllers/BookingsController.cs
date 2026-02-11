using Manruka.API.Data;
using Manruka.API.DTOs;
using Manruka.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manruka.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/bookings (Melihat semua daftar pinjaman)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User) // Join tabel User
                .Include(b => b.Room) // Join tabel Room
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    BorrowerName = b.User!.Name,
                    RoomName = b.Room!.Name,
                    Date = b.BookingDate.ToString("yyyy-MM-dd"),
                    Time = $"{b.StartTime:hh\\:mm} - {b.EndTime:hh\\:mm}",
                    Status = b.Status
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // POST: api/bookings (Mengajukan Pinjaman)
        [HttpPost]
        public async Task<ActionResult> CreateBooking(CreateBookingDto request)
        {
            // 1. Parsing Waktu (String "08:00" -> TimeSpan)
            if (!TimeSpan.TryParse(request.StartTime, out var start) || 
                !TimeSpan.TryParse(request.EndTime, out var end))
            {
                return BadRequest("Format jam salah. Gunakan HH:mm (contoh 09:30)");
            }

            if (start >= end)
            {
                return BadRequest("Jam selesai harus lebih besar dari jam mulai.");
            }

            // 2. CEK TABRAKAN JADWAL (Conflict Check)
            // Logika: Cari booking lain di Ruangan & Tanggal yang sama, 
            // dimana Statusnya 'Approved' (atau 'Pending' jika ingin ketat),
            // dan Waktunya beririsan.
            var isConflict = await _context.Bookings.AnyAsync(b => 
                b.RoomId == request.RoomId &&
                b.BookingDate.Date == request.BookingDate.Date &&
                b.Status == "Approved" && // Hanya yang sudah disetujui yang memblokir jadwal
                ((start < b.EndTime) && (end > b.StartTime))
            );

            if (isConflict)
            {
                return BadRequest("Maaf, ruangan sudah dipinjam (Approved) pada jam tersebut.");
            }

            // 3. Simpan Booking Baru
            var booking = new Booking
            {
                UserId = request.UserId,
                RoomId = request.RoomId,
                BookingDate = request.BookingDate,
                StartTime = start,
                EndTime = end,
                Purpose = request.Purpose,
                Status = "Pending" // Default Pending, menunggu Admin
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Peminjaman berhasil diajukan, menunggu persetujuan Admin." });
        }

        // PUT: api/bookings/{id}/status (Admin Approval)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status) 
        {
            // status input: "Approved" atau "Rejected"
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (status != "Approved" && status != "Rejected")
                return BadRequest("Status hanya boleh 'Approved' atau 'Rejected'");

            // Jika mau Approve, cek lagi apakah tiba-tiba ada yang Approved duluan
            if (status == "Approved")
            {
                var isConflict = await _context.Bookings.AnyAsync(b => 
                    b.Id != id && // Jangan cek diri sendiri
                    b.RoomId == booking.RoomId &&
                    b.BookingDate == booking.BookingDate &&
                    b.Status == "Approved" && 
                    ((booking.StartTime < b.EndTime) && (booking.EndTime > b.StartTime))
                );

                if (isConflict) return BadRequest("Gagal Approve. Ruangan sudah terisi oleh peminjam lain.");
            }

            booking.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Booking berhasil diubah menjadi {status}" });
        }
    }
}