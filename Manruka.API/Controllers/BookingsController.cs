using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Manruka.API.Data;
using Manruka.API.Entities;
using System.Globalization;

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

        // ---------------------------------------------------------
        // 1. GET: Ambil Semua Data
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.Id)
                .Select(b => new
                {
                    b.Id,
                    b.RoomId,
                    RoomName = b.Room.Name,
                    BorrowerName = b.User.Name,
                    // FIX: Format DateTime ke String
                    Date = b.BookingDate.ToString("yyyy-MM-dd"),
                    // FIX: Format TimeSpan ke String (hh:mm)
                    StartTime = b.StartTime.ToString(@"hh\:mm"),
                    EndTime = b.EndTime.ToString(@"hh\:mm"),
                    Time = $"{b.StartTime:hh\\:mm} - {b.EndTime:hh\\:mm}",
                    b.Status,
                    b.Purpose
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // ---------------------------------------------------------
        // 2. POST: Buat Booking Baru
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest request)
        {
            // Validasi User & Room
            var user = await _context.Users.FindAsync(request.UserId);
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (user == null || room == null) return BadRequest("User atau Ruangan tidak valid.");

            // FIX: Parsing ke DateTime & TimeSpan (Sesuai Entity Lama)
            if (!DateTime.TryParse(request.BookingDate, out DateTime dateVal) ||
                !TimeSpan.TryParse(request.StartTime, out TimeSpan startVal) ||
                !TimeSpan.TryParse(request.EndTime, out TimeSpan endVal))
            {
                return BadRequest("Format tanggal (YYYY-MM-DD) atau waktu (HH:mm) salah.");
            }

            if (startVal >= endVal) return BadRequest("Jam mulai harus sebelum jam selesai.");

            // Cek Bentrok (Conflict Check)
            bool isConflict = await _context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId &&
                b.BookingDate.Date == dateVal.Date && // Bandingkan Date-nya saja
                b.Status == "Approved" &&
                (b.StartTime < endVal && b.EndTime > startVal)
            );

            if (isConflict)
            {
                return BadRequest("Ruangan sudah terisi pada jam tersebut.");
            }

            var newBooking = new Booking
            {
                UserId = request.UserId,
                RoomId = request.RoomId,
                BookingDate = dateVal, // Masuk sebagai DateTime
                StartTime = startVal,  // Masuk sebagai TimeSpan
                EndTime = endVal,      // Masuk sebagai TimeSpan
                Purpose = request.Purpose,
                Status = "Pending"
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBookings), new { id = newBooking.Id }, newBooking);
        }

        // ---------------------------------------------------------
        // 3. PUT: Edit Booking
        // ---------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingRequest request)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound("Data peminjaman tidak ditemukan.");

            if (booking.Status != "Pending")
            {
                return BadRequest("Hanya peminjaman berstatus 'Pending' yang dapat diubah.");
            }

            // FIX: Parsing ke DateTime & TimeSpan
            if (!DateTime.TryParse(request.BookingDate, out DateTime dateVal) ||
                !TimeSpan.TryParse(request.StartTime, out TimeSpan startVal) ||
                !TimeSpan.TryParse(request.EndTime, out TimeSpan endVal))
            {
                return BadRequest("Format tanggal atau waktu salah.");
            }

            if (startVal >= endVal) return BadRequest("Jam mulai harus sebelum jam selesai.");

            // Cek Bentrok (Kecuali diri sendiri)
            bool isConflict = await _context.Bookings.AnyAsync(b =>
                b.Id != id &&
                b.RoomId == request.RoomId &&
                b.BookingDate.Date == dateVal.Date &&
                b.Status == "Approved" &&
                (b.StartTime < endVal && b.EndTime > startVal)
            );

            if (isConflict)
            {
                return BadRequest("Jadwal bentrok dengan peminjaman lain yang sudah disetujui.");
            }

            // Update Data
            booking.RoomId = request.RoomId;
            booking.BookingDate = dateVal;
            booking.StartTime = startVal;
            booking.EndTime = endVal;
            booking.Purpose = request.Purpose;
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Booking berhasil diperbarui." });
        }

        // ---------------------------------------------------------
        // 4. DELETE: Cancel Booking
        // ---------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound("Data tidak ditemukan.");

            if (booking.Status != "Pending")
            {
                return BadRequest("Hanya peminjaman berstatus 'Pending' yang dapat dibatalkan.");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking berhasil dibatalkan." });
        }

        // ---------------------------------------------------------
        // 5. UPDATE STATUS (Approval)
        // ---------------------------------------------------------
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound("Booking not found");

            if (status != "Approved" && status != "Rejected")
                return BadRequest("Invalid status. Use 'Approved' or 'Rejected'.");

            // Cek Konflik sebelum Approve
            if (status == "Approved")
            {
                bool isConflict = await _context.Bookings.AnyAsync(b =>
                    b.Id != id &&
                    b.RoomId == booking.RoomId &&
                    b.BookingDate.Date == booking.BookingDate.Date &&
                    b.Status == "Approved" &&
                    (booking.StartTime < b.EndTime && booking.EndTime > b.StartTime)
                );

                if (isConflict) return BadRequest("Gagal Approve. Ruangan sudah terisi oleh peminjam lain.");
            }

            booking.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Booking {status} successfully" });
        }
    }

    // FIX: Class DTO (Tambahkan inisialisasi string.Empty untuk cegah warning CS8618)
    public class BookingRequest
    {
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public string BookingDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }
}