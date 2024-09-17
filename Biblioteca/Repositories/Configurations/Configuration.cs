using Biblioteca.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biblioteca.Repositories.Configurations
{
    public class Configuration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.UserId).HasColumnName("UserId");
            builder.Property(x => x.Tipo).HasColumnName("Tipo");
            builder.Property(x => x.Name).HasColumnName("Name");
            builder.Property(x => x.LastName).HasColumnName("LastName");
            builder.Property(x => x.Email).HasColumnName("Email");
            builder.Property(x => x.DNI).HasColumnName("DNI");
            builder.Property(x => x.UserName).HasColumnName("UserName");
            builder.Property(x => x.Password).HasColumnName("Password");
        }

        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable("Book");
            builder.HasKey(x => x.BookId);
            builder.Property(x => x.BookId).HasColumnName("BookId");
            builder.Property(x => x.Tittle).HasColumnName("Tittle");
            builder.Property(x => x.Author).HasColumnName("Author");
            builder.Property(x => x.Gender).HasColumnName("Gender");
            builder.Property(x => x.Year).HasColumnName("Year");
            builder.Property(x => x.Cantidad).HasColumnName("Cantidad");
            builder.Property(x => x.Imagen).HasColumnName("Imagen");

        }

        public void Configure(EntityTypeBuilder<Solicitud> builder)
        {
            builder.ToTable("Solicitud");
            builder.HasKey(x => x.SolicitudId);
            builder.Property(x => x.SolicitudId).HasColumnName("SolicitudId");
            builder.Property(x => x.Tipo).HasColumnName("Tipo");
            builder.Property(x => x.Date).HasColumnName("Date");
            builder.Property(x => x.UserName).HasColumnName("UserName");
            builder.Property(x => x.Book).HasColumnName("Book");
            builder.Property(x => x.Gender).HasColumnName("Gender");
            builder.Property(x => x.Observation).HasColumnName("Observation");
        }

        public void Configure(EntityTypeBuilder<MovimientoLibro> builder)
        {
            builder.ToTable("MovimientoLibro");
            builder.HasKey(x => x.MovimientoLibroId);
            builder.Property(x => x.MovimientoLibroId).HasColumnName("MovimientoLibroId");
            builder.Property(x => x.BookId).HasColumnName("IdBook");
            builder.Property(x => x.Saldo).HasColumnName("Saldo");
        }
    }
}
