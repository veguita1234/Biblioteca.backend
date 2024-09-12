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
                .ToTable("User", "biblioteca");

            modelBuilder.Entity<Book>()
                .ToTable("Book", "biblioteca")
                .HasKey(b => b.BookId);

            modelBuilder.Entity<Book>()
                .Property(b => b.BookId)
                .HasColumnName("BookId");

            modelBuilder.Entity<Book>()
                .Property(b => b.Tittle)
                .HasColumnName("Tittle");

            modelBuilder.Entity<Book>()
                .Property(b => b.Author)
                .HasColumnName("Author");

            modelBuilder.Entity<Book>()
                .Property(b => b.Gender)
                .HasColumnName("Gender");

            modelBuilder.Entity<Book>()
                .Property(b => b.Year)
                .HasColumnName("Year");

            modelBuilder.Entity<Book>()
                .Property(b => b.Cantidad)
                .HasColumnName("Cantidad");

            modelBuilder.Entity<Book>()
                .Property(b => b.Imagen)
                .HasColumnName("Imagen");

            modelBuilder.Entity<Solicitud>()
                .ToTable("Solicitud", "biblioteca");

            modelBuilder.Entity<MovimientoLibro>()
           .ToTable("MovimientoLibro", "biblioteca");

            modelBuilder.Entity<MovimientoLibro>()
            .HasOne(m => m.Book)
            .WithMany(b => b.MovimientoLibros)
            .HasForeignKey(m => m.BookId);
        }

    }
}
