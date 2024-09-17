using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Repositories.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string Tipo { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string? DNI { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
