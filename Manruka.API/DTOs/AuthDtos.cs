namespace Manruka.API.DTOs
{
    public class RegisterDto
    {
        public required string Name { get; set; }
        public required string NRP { get; set; }
        public required string Department { get; set; }
        public required string Major { get; set; }
        public int YearEntry { get; set; }
        public required string Password { get; set; }
    }

    public class LoginDto
    {
        public required string NRP { get; set; }
        public required string Password { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}