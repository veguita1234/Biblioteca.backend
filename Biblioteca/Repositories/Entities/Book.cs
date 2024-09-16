using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Repositories.Entities
{
    public class Book
    {
        [Key]
        public Guid BookId { get; set; }
        public string Tittle {  get; set; }
        public string? Author { get; set; }
        public string Gender { get; set; }
        public string? Year { get; set; }
        public int Cantidad { get; set; }
        public string? Imagen { get; set; }

        public ICollection<MovimientoLibro> MovimientoLibros { get; set; }
    }
}
