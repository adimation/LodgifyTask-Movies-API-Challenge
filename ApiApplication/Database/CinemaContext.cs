using ApiApplication.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiApplication.Database
{
    public class CinemaContext : DbContext
    {
        public CinemaContext(DbContextOptions<CinemaContext> options) : base(options)
        {
            
        }

        public DbSet<AuditoriumEntity> Auditoriums { get; set; }
        public DbSet<ShowtimeEntity> Showtimes { get; set; }
        public DbSet<MovieEntity> Movies { get; set; }
        public DbSet<TicketEntity> Tickets { get; set; }
        public DbSet<SeatEntity> Seats { get; set; }
        public DbSet<TicketSeatEntity> TicketsSeats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditoriumEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();
                build.HasMany(entry => entry.Showtimes).WithOne().HasForeignKey(entity => entity.AuditoriumId);
            });

            modelBuilder.Entity<SeatEntity>(build =>
            {
                build.HasKey(entry => new { entry.AuditoriumId, entry.Row, entry.SeatNumber });
                build.HasOne(entry => entry.Auditorium).WithMany(entry => entry.Seats).HasForeignKey(entry => entry.AuditoriumId);
            });

            modelBuilder.Entity<ShowtimeEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();
                build.HasOne(entry => entry.Movie).WithMany(entry => entry.Showtimes);
                build.HasMany(entry => entry.Tickets).WithOne(entry => entry.Showtime).HasForeignKey(entry => entry.ShowtimeId);
            });

            modelBuilder.Entity<MovieEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<TicketEntity>(build =>
            {
                build.HasKey(entry => entry.Id);
                build.Property(entry => entry.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<TicketSeatEntity>(build =>
            {
                //build.Property(entry => entry.Id).ValueGeneratedOnAdd();
                build.HasKey(ts => new { ts.TicketId, ts.AuditoriumId, ts.Row, ts.SeatNumber });
                build.HasOne(ts => ts.Ticket).WithMany(t => t.TicketSeats).HasForeignKey(ts => ts.TicketId);
                build.HasOne(ts => ts.Seat).WithMany(s => s.TicketSeats).HasForeignKey(ts => new { ts.AuditoriumId, ts.Row, ts.SeatNumber });
            });
        }
    }
}
