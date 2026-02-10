using Manruka.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Manruka.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Pastikan NRP Unik
            modelBuilder.Entity<User>()
                .HasIndex(u => u.NRP)
                .IsUnique();
        }
    }
}