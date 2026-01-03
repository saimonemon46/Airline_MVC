using Air.Models;
using Microsoft.EntityFrameworkCore;

namespace Air.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Airplane> Airplanes { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Booking ↔ User
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking ↔ Airplane
            builder.Entity<Booking>()
                .HasOne(b => b.Airplane)           // use the new navigation property name
                .WithMany(a => a.Bookings)         // collection in Airplane
                .HasForeignKey(b => b.AirplaneId)  // FK column
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
