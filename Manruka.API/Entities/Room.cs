namespace Manruka.API.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public required string Name { get; set; } // Contoh: "Laboratorium RPL"
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; } = true; // Default Aktif
    }
}