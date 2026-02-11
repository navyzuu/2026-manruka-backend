using Manruka.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Manruka.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Pastikan NRP Unik
            modelBuilder.Entity<User>()
                .HasIndex(u => u.NRP)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingDate)
                .HasColumnType("date");

            // Seed Data Ruangan Awal
            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Name = "C-301", Capacity = 60, Description = "Lantai 3", IsAvailable = true },
                new Room { Id = 2, Name = "C-302", Capacity = 60, Description = "Lantai 3", IsAvailable = true },
                new Room { Id = 3, Name = "C-105", Capacity = 30, Description = "Lantai 1", IsAvailable = true },
                new Room { Id = 4, Name = "C-103", Capacity = 30, Description = "Lantai 1", IsAvailable = true },
                new Room { Id = 5, Name = "B-204", Capacity = 60, Description = "Lantai 2", IsAvailable = true }
            );

            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 99, // Gunakan ID 99 agar tidak bentrok dengan user yang sudah Anda buat
                    Name = "Administrator",
                    NRP = "admin", // Login pakai ini
                    Department = "IT Support",
                    Major = "-",
                    YearEntry = 2020,
                    Role = "Admin",
                    PasswordHash = "admin123"
                }
            );
        }
    }
}