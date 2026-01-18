using Microsoft.EntityFrameworkCore;
using TourismManagementSystem.Models;

namespace TourismManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Transport> Transports { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}