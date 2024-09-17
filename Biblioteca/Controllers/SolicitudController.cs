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
        //[HttpPost("crearSolicitud")]
        //public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudDTO solicitudDto)
        //{
        //    if (solicitudDto == null)
        //    {
        //        return BadRequest(new { Success = false, Message = "La solicitud no puede ser nula." });
        //    }

        //    if (string.IsNullOrEmpty(solicitudDto.UserName) || string.IsNullOrEmpty(solicitudDto.Book))
        //    {
        //        return BadRequest(new { Success = false, Message = "El nombre de usuario y el título del libro son requeridos." });
        //    }

        //    var user = await _context.User.SingleOrDefaultAsync(u => u.UserName == solicitudDto.UserName);
        //    if (user == null)
        //    {
        //        return BadRequest(new { Success = false, Message = "El usuario no existe." });
        //    }

        //    var libroBuscado = solicitudDto.Book.Trim().ToLower();
        //    var movimientoLibro = await _context.MovimientoLibro
        //        .Include(m => m.Book)
        //        .FirstOrDefaultAsync(m => m.Book.Tittle.Trim().ToLower() == libroBuscado);

        //    if (movimientoLibro == null)
        //    {
        //        return BadRequest(new { Success = false, Message = "El libro no existe." });
        //    }

        //    if (solicitudDto.Tipo == "Pedir")
        //    {
        //        if (movimientoLibro.Saldo > 0 && movimientoLibro.Book.Cantidad > 0)
        //        {
        //            movimientoLibro.Saldo -= 1;
        //            movimientoLibro.Book.Cantidad -= 1;
        //        }
        //        else
        //        {
        //            return BadRequest(new { Success = false, Message = "No hay suficiente saldo para el libro." });
        //        }
        //    }
        //    else if (solicitudDto.Tipo == "Regresar")
        //    {
        //        var solicitudPedirPrevia = await _context.Solicitud
        //            .Where(s => s.UserName == solicitudDto.UserName &&
        //                        s.Book == solicitudDto.Book &&
        //                        s.Tipo == "Pedir")
        //            .ToListAsync();

        //        var solicitudRegresarPrevia = await _context.Solicitud
        //            .Where(s => s.UserName == solicitudDto.UserName &&
        //                        s.Book == solicitudDto.Book &&
        //                        s.Tipo == "Regresar")
        //            .ToListAsync();

        //        if (!solicitudPedirPrevia.Any())
        //        {
        //            return BadRequest(new { Success = false, Message = "No puedes regresar un libro que no has pedido." });
        //        }

        //        if (solicitudRegresarPrevia.Count >= solicitudPedirPrevia.Count)
        //        {
        //            return BadRequest(new { Success = false, Message = "No puedes regresar más libros de los que has pedido." });
        //        }

        //        movimientoLibro.Saldo += 1;
        //        movimientoLibro.Book.Cantidad += 1;
        //    }
        //    else
        //    {
        //        return BadRequest(new { Success = false, Message = "Tipo de solicitud no válido." });
        //    }

        //    var nuevaSolicitud = new Solicitud
        //    {
        //        SolicitudId = Guid.NewGuid(),
        //        Tipo = solicitudDto.Tipo,
        //        Date = DateTime.Now,
        //        UserName = solicitudDto.UserName,
        //        Book = movimientoLibro.Book.Tittle,
        //        Observation = solicitudDto.Observation
        //    };

        //    _context.Solicitud.Add(nuevaSolicitud);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { Success = true, Message = "Solicitud procesada exitosamente." });
        //}

        [HttpPost("crearSolicitud")]
        public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudDTO solicitudDto)
        {
            if (solicitudDto == null)
            {
                return BadRequest(new { Success = false, Message = "La solicitud no puede ser nula." });
            }

            if (string.IsNullOrEmpty(solicitudDto.UserName) || string.IsNullOrEmpty(solicitudDto.Book) || string.IsNullOrEmpty(solicitudDto.Gender))
            {
                return BadRequest(new { Success = false, Message = "El nombre de usuario, el título del libro y el género son requeridos." });
            }

            var user = await _context.User.SingleOrDefaultAsync(u => u.UserName == solicitudDto.UserName);
            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "El usuario no existe." });
            }

            var libroBuscado = solicitudDto.Book.Trim().ToLower();
            var generoBuscado = solicitudDto.Gender.Trim().ToLower();

            var movimientoLibro = await _context.MovimientoLibro
                .Include(m => m.Book)
                .FirstOrDefaultAsync(m =>
                    m.Book.Tittle.Trim().ToLower() == libroBuscado &&
                    m.Book.Gender.Trim().ToLower() == generoBuscado);

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

            // Obtener solicitudes de pedir
            var solicitudesPedir = await _context.Solicitud
                .Join(_context.MovimientoLibro,
                      s => s.Book,
                      m => m.Book.Tittle,
                      (s, m) => new { Solicitud = s, Movimiento = m })
                .Where(sm => sm.Solicitud.Tipo == "Pedir" && sm.Solicitud.UserName == userName)
                .GroupBy(sm => new { sm.Movimiento.Book.Gender, sm.Solicitud.Book })
                .Select(g => new
                {
                    Book = g.Key.Book,
                    Gender = g.Key.Gender,
                    CantidadPedir = g.Count()
                })
                .ToListAsync();

            // Obtener solicitudes de regresar
            var solicitudesRegresar = await _context.Solicitud
                .Join(_context.MovimientoLibro,
                      s => s.Book,
                      m => m.Book.Tittle,
                      (s, m) => new { Solicitud = s, Movimiento = m })
                .Where(sm => sm.Solicitud.Tipo == "Regresar" && sm.Solicitud.UserName == userName)
                .GroupBy(sm => new { sm.Movimiento.Book.Gender, sm.Solicitud.Book })
                .Select(g => new
                {
                    Book = g.Key.Book,
                    Gender = g.Key.Gender,
                    CantidadRegresar = g.Count()
                })
                .ToListAsync();

            // Crear un diccionario para contar las solicitudes de regresar
            var dictRegresar = solicitudesRegresar.ToDictionary(sr => (sr.Book, sr.Gender), sr => sr.CantidadRegresar);

            // Filtrar los libros que tienen solicitudes de pedir menos o igual que las solicitudes de regresar
            var librosParaDevolver = solicitudesPedir
                .Where(sp => !dictRegresar.ContainsKey((sp.Book, sp.Gender)) || sp.CantidadPedir > dictRegresar[(sp.Book, sp.Gender)])
                .Select(sp => new { Title = sp.Book, Gender = sp.Gender })
                .ToList();

            // Excluir libros que ya fueron devueltos en solicitudes anteriores
            var librosDevueltos = await _context.Solicitud
                .Join(_context.MovimientoLibro,
                      s => s.Book,
                      m => m.Book.Tittle,
                      (s, m) => new { Solicitud = s, Movimiento = m })
                .Where(sm => sm.Solicitud.Tipo == "Regresar" && sm.Solicitud.UserName == userName)
                .GroupBy(sm => new { sm.Movimiento.Book.Gender, sm.Solicitud.Book })
                .Select(g => new
                {
                    Book = g.Key.Book,
                    Gender = g.Key.Gender,
                    CantidadDevuelta = g.Count()
                })
                .ToListAsync();

            var dictDevueltos = librosDevueltos.ToDictionary(ld => (ld.Book, ld.Gender), ld => ld.CantidadDevuelta);

            librosParaDevolver = librosParaDevolver
                .Where(lp => !dictDevueltos.ContainsKey((lp.Title, lp.Gender)) || dictDevueltos[(lp.Title, lp.Gender)] < solicitudesPedir.FirstOrDefault(sp => sp.Book == lp.Title && sp.Gender == lp.Gender)?.CantidadPedir)
                .ToList();

            return Ok(new { Success = true, libros = librosParaDevolver });
        }
        //[HttpGet("obtenerLibrosParaDevolver")]
        //public async Task<IActionResult> ObtenerLibrosParaDevolver([FromQuery] string userName)
        //{
        //    if (string.IsNullOrEmpty(userName))
        //    {
        //        return BadRequest(new { Success = false, Message = "El nombre de usuario es requerido." });
        //    }

        //    // Obtener solicitudes de pedir
        //    var solicitudesPedir = await _context.Solicitud
        //        .Join(_context.MovimientoLibro,
        //              s => s.Book,
        //              m => m.Book.Tittle,
        //              (s, m) => new { Solicitud = s, Movimiento = m })
        //        .Where(sm => sm.Solicitud.Tipo == "Pedir" && sm.Solicitud.UserName == userName)
        //        .GroupBy(sm => new { sm.Movimiento.Book.Gender, sm.Solicitud.Book })
        //        .Select(g => new
        //        {
        //            Book = g.Key.Book,
        //            Gender = g.Key.Gender,
        //            CantidadPedir = g.Count()
        //        })
        //        .ToListAsync();

        //    // Obtener solicitudes de regresar
        //    var solicitudesRegresar = await _context.Solicitud
        //        .Join(_context.MovimientoLibro,
        //              s => s.Book,
        //              m => m.Book.Tittle,
        //              (s, m) => new { Solicitud = s, Movimiento = m })
        //        .Where(sm => sm.Solicitud.Tipo == "Regresar" && sm.Solicitud.UserName == userName)
        //        .GroupBy(sm => new { sm.Movimiento.Book.Gender, sm.Solicitud.Book })
        //        .Select(g => new
        //        {
        //            Book = g.Key.Book,
        //            Gender = g.Key.Gender,
        //            CantidadRegresar = g.Count()
        //        })
        //        .ToListAsync();

        //    // Crear un diccionario para contar las solicitudes de regresar
        //    var dictRegresar = solicitudesRegresar.ToDictionary(sr => (sr.Book, sr.Gender), sr => sr.CantidadRegresar);

        //    // Filtrar los libros que tienen solicitudes de pedir menos o igual que las solicitudes de regresar
        //    var librosParaDevolver = solicitudesPedir
        //        .Where(sp => !dictRegresar.ContainsKey((sp.Book, sp.Gender)) || sp.CantidadPedir > dictRegresar[(sp.Book, sp.Gender)])
        //        .Select(sp => new { Title = sp.Book, Gender = sp.Gender })
        //        .ToList();

        //    // Excluir libros que ya fueron devueltos en solicitudes anteriores
        //    var librosDevueltos = await _context.Solicitud
        //        .Join(_context.MovimientoLibro,
        //              s => s.Book,
        //              m => m.Book.Tittle,
        //              (s, m) => new { Solicitud = s, Movimiento = m })
        //        .Where(sm => sm.Solicitud.Tipo == "Regresar" && sm.Solicitud.UserName == userName)
        //        .GroupBy(sm => new { sm.Movimiento.Book.Gender, sm.Solicitud.Book })
        //        .Select(g => new
        //        {
        //            Book = g.Key.Book,
        //            Gender = g.Key.Gender,
        //            CantidadDevuelta = g.Count()
        //        })
        //        .ToListAsync();

        //    var dictDevueltos = librosDevueltos.ToDictionary(ld => (ld.Book, ld.Gender), ld => ld.CantidadDevuelta);

        //    librosParaDevolver = librosParaDevolver
        //        .Where(lp => !dictDevueltos.ContainsKey((lp.Title, lp.Gender)) || dictDevueltos[(lp.Title, lp.Gender)] < solicitudesPedir.FirstOrDefault(sp => sp.Book == lp.Title && sp.Gender == lp.Gender)?.CantidadPedir)
        //        .ToList();

        //    return Ok(new { Success = true, libros = librosParaDevolver });
        //}


    }
}
