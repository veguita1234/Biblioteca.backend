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
            var user = await _context.User.SingleOrDefaultAsync(u => u.UserName == solicitudDto.UserName);

            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "El usuario no existe." });
            }

            var movimientoLibro = await _context.MovimientoLibro
                .Include(m => m.Book)
                .FirstOrDefaultAsync(m => m.Book.Tittle == solicitudDto.Book);

            if (movimientoLibro == null)
            {
                return BadRequest(new { Success = false, Message = "El libro no existe." });
            }

            if (solicitudDto.Tipo == "Pedir")
            {
                if (movimientoLibro.Saldo > 0 && movimientoLibro.Book.Cantidad > 0)
                {
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
                var solicitudPedirPrevia = await _context.Solicitud
                    .Where(s => s.UserName == solicitudDto.UserName &&
                                s.Book == solicitudDto.Book &&
                                s.Tipo == "Pedir")
                    .ToListAsync();

                var solicitudRegresarPrevia = await _context.Solicitud
                    .Where(s => s.UserName == solicitudDto.UserName &&
                                s.Book == solicitudDto.Book &&
                                s.Tipo == "Regresar")
                    .ToListAsync();

                if (!solicitudPedirPrevia.Any())
                {
                    return BadRequest(new { Success = false, Message = "No puedes regresar un libro que no has pedido." });
                }

                if (solicitudRegresarPrevia.Count >= solicitudPedirPrevia.Count)
                {
                    return BadRequest(new { Success = false, Message = "No puedes regresar más libros de los que has pedido." });
                }

                movimientoLibro.Saldo += 1;
                movimientoLibro.Book.Cantidad += 1;
            }
            else
            {
                return BadRequest(new { Success = false, Message = "Tipo de solicitud no válido." });
            }

            var nuevaSolicitud = new Solicitud
            {
                SolicitudId = Guid.NewGuid(),
                Tipo = solicitudDto.Tipo,
                Date = DateTime.Now,
                UserName = solicitudDto.UserName,
                Book = movimientoLibro.Book.Tittle,
                Observation = solicitudDto.Observation
            };

            _context.Solicitud.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Solicitud procesada exitosamente." });
        }


        [HttpGet("obtenerSolicitudes")]
        public async Task<IActionResult> ObtenerSolicitudes([FromQuery] string tipo, [FromQuery] string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest(new { Success = false, Message = "El nombre de usuario es requerido." });
            }

            var solicitudes = await _context.Solicitud
                .Where(s => s.Tipo == tipo && s.UserName == userName)
                .ToListAsync();

            var librosPedidos = solicitudes
                .GroupBy(s => s.Book)
                .Select(g => new { Tittle = g.Key })
                .ToList();

            return Ok(new { Success = true, libros = librosPedidos });
        }

        [HttpGet("obtenerLibrosParaDevolver")]
        public async Task<IActionResult> ObtenerLibrosParaDevolver([FromQuery] string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest(new { Success = false, Message = "El nombre de usuario es requerido." });
            }

            // Obtener todas las solicitudes de tipo "Pedir" del usuario
            var solicitudesPedir = await _context.Solicitud
                .Where(s => s.Tipo == "Pedir" && s.UserName == userName)
                .ToListAsync();

            // Obtener todas las solicitudes de tipo "Regresar" del usuario
            var solicitudesRegresar = await _context.Solicitud
                .Where(s => s.Tipo == "Regresar" && s.UserName == userName)
                .ToListAsync();

            // Filtrar los libros que aún no han sido devueltos completamente
            var librosParaDevolver = solicitudesPedir
                .GroupBy(s => s.Book)
                .Where(g => g.Count() > solicitudesRegresar.Count(sr => sr.Book == g.Key))
                .Select(g => new { Title = g.Key })
                .ToList();

            return Ok(new { Success = true, libros = librosParaDevolver });
        }

    }
}
