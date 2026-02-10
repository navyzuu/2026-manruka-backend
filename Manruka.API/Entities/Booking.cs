using System.ComponentModel.DataAnnotations.Schema;

namespace Manruka.API.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        
        // Relasi ke User (Peminjam)
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // Relasi ke Room (Ruangan)
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        // Data Waktu
        public DateTime BookingDate { get; set; } // Tanggal Peminjaman (YYYY-MM-DD)
        public TimeSpan StartTime { get; set; }   // Jam Mulai (HH:mm)
        public TimeSpan EndTime { get; set; }     // Jam Selesai (HH:mm)

        public required string Purpose { get; set; } // Keperluan
        
        // Status: Pending, Approved, Rejected
        public string Status { get; set; } = "Pending"; 
    }
}