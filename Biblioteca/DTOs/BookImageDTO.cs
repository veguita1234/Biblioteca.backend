namespace Biblioteca.DTOs
{
    public class BookImageDTO
    {
        public IFormFile? ImageFile { get; set; } // Para archivos locales
        public string? ImageUrl { get; set; } // Para URLs
    }
}
