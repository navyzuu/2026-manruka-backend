namespace Manruka.API.DTOs
{
    public class CreateRoomDto
    {
        public required string Name { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }
}