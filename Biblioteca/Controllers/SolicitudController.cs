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
            if (solicitudDto == null)
            {
                return BadRequest(new { Success = false, Message = "La solicitud no puede ser nula." });
            }

            if (solicitudDto.Tipo == "Pedir")
            {
                if (string.IsNullOrEmpty(solicitudDto.UserName) || solicitudDto.BookId == Guid.Empty)
                {
                    return BadRequest(new { Success = false, Message = "El nombre de usuario y el ID del libro son requeridos." });
                }

                var user = await _context.User.SingleOrDefaultAsync(u => u.UserName == solicitudDto.UserName);
                if (user == null)
                {
                    return BadRequest(new { Success = false, Message = "El usuario no existe." });
                }

                var book = await _context.Book
                    .SingleOrDefaultAsync(b => b.BookId == solicitudDto.BookId);

                if (book == null)
                {
                    return BadRequest(new { Success = false, Message = "El libro no existe." });
                }

                var movimientoLibro = await _context.MovimientoLibro
                    .FirstOrDefaultAsync(m => m.BookId == solicitudDto.BookId);

                if (movimientoLibro == null || movimientoLibro.Saldo <= 0 || book.Cantidad <= 0)
                {
                    return BadRequest(new { Success = false, Message = "No hay suficiente saldo para el libro." });
                }

                movimientoLibro.Saldo -= 1;
                book.Cantidad -= 1;

                var nuevaSolicitud = new Solicitud
                {
                    SolicitudId = Guid.NewGuid(),
                    Tipo = solicitudDto.Tipo,
                    Date = DateTime.Now,
                    UserName = solicitudDto.UserName,
                    BookId = book.BookId,
                    Book = book.Tittle, 
                    Gender = book.Gender,
                    Observation = solicitudDto.Observation
                };

                _context.Solicitud.Add(nuevaSolicitud);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Solicitud de pedido procesada exitosamente." });
            }
            else if (solicitudDto.Tipo == "Regresar")
            {
                if (string.IsNullOrEmpty(solicitudDto.Book) || string.IsNullOrEmpty(solicitudDto.Gender))
                {
                    return BadRequest(new { Success = false, Message = "El título y el género del libro son requeridos." });
                }

                var movimientoLibro = await _context.MovimientoLibro
                    .Include(m => m.Book)
                    .FirstOrDefaultAsync(m => m.Book.Tittle == solicitudDto.Book && m.Book.Gender == solicitudDto.Gender);

                if (movimientoLibro == null)
                {
                    return BadRequest(new { Success = false, Message = "El libro con el título y género especificados no existe." });
                }

                movimientoLibro.Saldo += 1;
                movimientoLibro.Book.Cantidad += 1;

                var nuevaSolicitud = new Solicitud
                {
                    SolicitudId = Guid.NewGuid(),
                    Tipo = solicitudDto.Tipo,
                    Date = DateTime.Now,
                    UserName = solicitudDto.UserName, 
                    Book = solicitudDto.Book,
                    BookId = movimientoLibro.BookId,
                    Gender = solicitudDto.Gender,
                    Observation = solicitudDto.Observation
                };

                _context.Solicitud.Add(nuevaSolicitud);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Solicitud de regreso procesada exitosamente." });
            }
            else
            {
                return BadRequest(new { Success = false, Message = "Tipo de solicitud no válido." });
            }
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

            var solicitudesPedir = await _context.Solicitud
                .Where(s => s.Tipo == "Pedir" && s.UserName == userName)
                .Select(s => s.BookId)
                .ToListAsync();

            var solicitudesRegresar = await _context.Solicitud
                .Where(s => s.Tipo == "Regresar" && s.UserName == userName)
                .Select(s => s.BookId)
                .ToListAsync();

            var dictRegresar = solicitudesRegresar
                .Where(bookId => bookId != null)
                .GroupBy(sr => sr)
                .ToDictionary(g => g.Key, g => g.Count());

            var librosParaDevolver = solicitudesPedir
                .Where(bookId => bookId != null)
                .GroupBy(sp => sp)
                .Where(g => !dictRegresar.ContainsKey(g.Key) || g.Count() > dictRegresar[g.Key]) 
                .Select(g => g.Key)
                .ToList();

            var librosDevueltos = await _context.Solicitud
                .Where(s => s.Tipo == "Regresar" && s.UserName == userName)
                .GroupBy(s => s.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    CantidadDevuelta = g.Count()
                })
                .ToListAsync();

            var dictDevueltos = librosDevueltos
                .Where(ld => ld.BookId != null)
                .ToDictionary(ld => ld.BookId, ld => ld.CantidadDevuelta);

            librosParaDevolver = librosParaDevolver
                .Where(lp => !dictDevueltos.ContainsKey(lp) || dictDevueltos[lp] < solicitudesPedir.Count(sp => sp == lp))
                .ToList();

            var librosParaDevolverConDetalles = await _context.Book
                .Where(b => librosParaDevolver.Contains(b.BookId))
                .Select(b => new
                {
                    BookId = b.BookId,
                    Title = b.Tittle ?? "Título no disponible",
                    Gender = b.Gender ?? "Género no disponible"
                })
                .ToListAsync();

            return Ok(new { Success = true, libros = librosParaDevolverConDetalles });
        }









    }
}
