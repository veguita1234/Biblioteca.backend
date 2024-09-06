using Biblioteca.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Book> Book {  get; set; }
        public DbSet<Solicitud> Solicitud { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<MovimientoLibro> MovimientoLibro { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("User");

            modelBuilder.Entity<Book>()
                .ToTable("Book");

            modelBuilder.Entity<Solicitud>()
                .ToTable("Solicitud");

            modelBuilder.Entity<MovimientoLibro>()
                .ToTable("MovimientoLibro");
        }

    }
}
