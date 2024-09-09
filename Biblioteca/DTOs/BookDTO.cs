using System.ComponentModel.DataAnnotations;

namespace Biblioteca.DTOs
{
    public class BookDTO
    {
        [Key]
        public Guid BookId { get; set; }
        public string Tittle { get; set; }
        public string Author { get; set; }
        public string Gender { get; set; }
        public string Year { get; set; }
        public int Cantidad { get; set; }
        public string Imagen { get; set; }
        
    }
}
