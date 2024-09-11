using Biblioteca.DTOs;
using Biblioteca.Repositories;
using Biblioteca.Repositories.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientoLibroController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimientoLibroController(AppDbContext context)
        {
            _context = context;
        }



        [HttpGet("getMovimientoLibros")]
        public async Task<IActionResult> GetMovimientoLibros()
        {
            var movimientos = await _context.MovimientoLibro
                                            .Include(m => m.Book) 
                                            .ToListAsync();

            var movimientosDto = movimientos.Select(m => new
            {
                MovimientoLibroId = m.MovimientoLibroId,
                BookId = m.BookId,
                Saldo = m.Saldo,
                BookTitle = m.Book.Tittle, 
                BookAuthor = m.Book.Author
            }).ToList();

            return Ok(new { Success = true, Data = movimientosDto });
        }

        
    }
}