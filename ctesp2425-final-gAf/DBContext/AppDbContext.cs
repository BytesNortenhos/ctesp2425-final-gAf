using Microsoft.EntityFrameworkCore;
using ctesp2425_final_gAf.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Reservation>()
            .ToTable("dsosReservation");

        modelBuilder.Entity<Reservation>()
            .Property(r => r.CreatedAt)
            .HasColumnType("datetime2");
    }
}
