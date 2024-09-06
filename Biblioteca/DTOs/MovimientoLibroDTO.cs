using System.ComponentModel.DataAnnotations;

namespace Biblioteca.DTOs
{
    public class MovimientoLibroDTO
    {
        [Key]
        public Guid MovimientoLibroId { get; set; }
        public Guid BookId { get; set; }
        public int Saldo { get; set; }
    }
}
