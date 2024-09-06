using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Repositories.Entities
{
    public class MovimientoLibro
    {
        [Key]
        public Guid MovimientoLibroId { get; set; }
        public Guid BookId { get; set; }
        public int Saldo { get; set; }

        public Book Book { get; set; }
    }
}
