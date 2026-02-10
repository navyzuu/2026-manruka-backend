namespace Manruka.API.DTOs
{
    public class CreateBookingDto
    {
        public int UserId { get; set; } // Nanti ini otomatis dari token, skrg manual dulu
        public int RoomId { get; set; }
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; } = "08:00"; // Format string "HH:mm" agar mudah diinput
        public string EndTime { get; set; } = "10:00";
        public required string Purpose { get; set; }
    }

    public class BookingDto
    {
        public int Id { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty; // "08:00 - 10:00"
        public string Status { get; set; } = string.Empty;
    }
}