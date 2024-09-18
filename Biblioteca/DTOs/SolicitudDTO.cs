using System.ComponentModel.DataAnnotations;

namespace Biblioteca.DTOs
{
    public class SolicitudDTO
    {
        public Guid SolicitudId { get; set; }
        public string Tipo { get; set; }
        public DateTime Date { get; set; }
        public string UserName { get; set; }
        public string Book { get; set; }
        public Guid BookId { get; set; }
        public string? Gender { get; set; }
        public string? Observation { get; set; }
    }

}
