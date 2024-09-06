using Biblioteca.DTOs;
using Biblioteca.Repositories;
using Biblioteca.Repositories.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitudController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public SolicitudController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("crearSolicitud")]
        public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudDTO solicitudDto)
        {
            // Verificar si el usuario existe por el UserName en la solicitud
            var user = await _context.User.SingleOrDefaultAsync(u => u.UserName == solicitudDto.UserName);

            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "El usuario no existe." });
            }

            // Verificar si existe el libro por el nombre en la solicitud
            var movimientoLibro = await _context.MovimientoLibro
                .Include(m => m.Book)
                .FirstOrDefaultAsync(m => m.Book.Tittle == solicitudDto.Book);

            if (movimientoLibro == null)
            {
                return BadRequest(new { Success = false, Message = "El libro no existe." });
            }

            if (solicitudDto.Tipo == "Pedir")
            {
                // Verificar si hay saldo disponible
                if (movimientoLibro.Saldo > 0 && movimientoLibro.Book.Cantidad > 0)
                {
                    // Restar 1 al saldo y a la cantidad del libro
                    movimientoLibro.Saldo -= 1;
                    movimientoLibro.Book.Cantidad -= 1;
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "No hay suficiente saldo para el libro." });
                }
            }
            else if (solicitudDto.Tipo == "Regresar")
            {
                // Verificar si el usuario ha realizado previamente una solicitud de tipo "Pedir" para el mismo libro
                var solicitudPedirPrevia = await _context.Solicitud
                    .Where(s => s.UserName == solicitudDto.UserName &&
                                s.Book == solicitudDto.Book &&
                                s.Tipo == "Pedir")
                    .ToListAsync();

                // Verificar si ya hay una solicitud de tipo "Regresar" correspondiente
                var solicitudRegresarPrevia = await _context.Solicitud
                    .Where(s => s.UserName == solicitudDto.UserName &&
                                s.Book == solicitudDto.Book &&
                                s.Tipo == "Regresar")
                    .ToListAsync();

                // Si el usuario nunca ha hecho un "Pedir" para este libro
                if (!solicitudPedirPrevia.Any())
                {
                    return BadRequest(new { Success = false, Message = "No puedes regresar un libro que no has pedido." });
                }

                // Si ya se han hecho más "Regresar" que "Pedir" (lo cual no debería ser posible)
                if (solicitudRegresarPrevia.Count >= solicitudPedirPrevia.Count)
                {
                    return BadRequest(new { Success = false, Message = "No puedes regresar más libros de los que has pedido." });
                }

                // Incrementar el saldo y la cantidad del libro en 1
                movimientoLibro.Saldo += 1;
                movimientoLibro.Book.Cantidad += 1;
            }
            else
            {
                return BadRequest(new { Success = false, Message = "Tipo de solicitud no válido." });
            }

            // Crear una nueva solicitud
            var nuevaSolicitud = new Solicitud
            {
                SolicitudId = Guid.NewGuid(),
                Tipo = solicitudDto.Tipo,
                Date = DateTime.Now,
                UserName = solicitudDto.UserName,  // El UserName que fue validado
                Book = movimientoLibro.Book.Tittle,  // Guardar el título del libro
                Observation = solicitudDto.Observation
            };

            // Agregar la solicitud al contexto
            _context.Solicitud.Add(nuevaSolicitud);

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Solicitud procesada exitosamente." });
        }


    }
}
