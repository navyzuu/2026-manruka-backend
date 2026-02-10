namespace Manruka.API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string NRP { get; set; } // Kosong jika Admin
        public string? Department { get; set; }  // Departemen (Opsional buat admin)
        public string? Major { get; set; }       // Jurusan
        public int YearEntry { get; set; }
        public required string Role { get; set; } // "Admin" atau "User"
        public required string PasswordHash { get; set; }
    }
}