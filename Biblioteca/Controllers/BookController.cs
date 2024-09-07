using Biblioteca.DTOs;
using Biblioteca.Repositories;
using Biblioteca.Repositories.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public BookController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPost("addbook")]
        public async Task<IActionResult> AddBook(BookDTO bookDTO)
        {
            // Validar el DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            try
            {
                // Convertir de BookDTO a la entidad Book
                var book = new Book
                {
                    BookId = Guid.NewGuid(),
                    Tittle = bookDTO.Tittle,
                    Author = bookDTO.Author,
                    Gender = bookDTO.Gender,
                    Year = bookDTO.Year,
                    Cantidad = bookDTO.Cantidad,
                };

                // Guardar el libro en la base de datos
                _context.Book.Add(book);
                await _context.SaveChangesAsync();

                // Crear un nuevo MovimientoLibro con el BookId y la Cantidad como Saldo
                var movimientoLibro = new MovimientoLibro
                {
                    MovimientoLibroId = Guid.NewGuid(),
                    BookId = book.BookId,
                    Saldo = book.Cantidad
                };

                // Guardar el MovimientoLibro en la base de datos
                _context.MovimientoLibro.Add(movimientoLibro);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Registro exitoso y movimiento registrado.", BookId = book.BookId });
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que pueda ocurrir
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Error al agregar el libro.",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("books")]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _context.Book.ToListAsync();

                var bookDto = books.Select(u => new BookDTO
                {
                    BookId = u.BookId,
                    Tittle = u.Tittle,
                    Author = u.Author,
                    Gender = u.Gender,
                    Year = u.Year,
                    Cantidad = u.Cantidad,
                }).ToList();

                return Ok(new
                {
                    Success = true,
                    books = bookDto
                });
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que pueda ocurrir
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Se produjo un error al obtener los libros.",
                    Error = ex.Message
                });
            }
        }

        [HttpPut("updatebook/{id}")]
        public async Task<IActionResult> UpdateBook(Guid id, BookDTO bookDTO)
        {
            // Validar el DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            try
            {
                // Buscar el libro por su BookId
                var book = await _context.Book.FindAsync(id);

                if (book == null)
                {
                    return NotFound(new { Success = false, Message = "Libro no encontrado." });
                }

                // Actualizar las propiedades del libro con los valores del DTO
                book.Tittle = bookDTO.Tittle;
                book.Author = bookDTO.Author;
                book.Gender = bookDTO.Gender;
                book.Year = bookDTO.Year;
                book.Cantidad = bookDTO.Cantidad;

                // Actualizar el libro en la base de datos
                _context.Book.Update(book);

                // Si deseas también actualizar el saldo en el MovimientoLibro relacionado
                var movimientoLibro = await _context.MovimientoLibro.FirstOrDefaultAsync(m => m.BookId == id);
                if (movimientoLibro != null)
                {
                    movimientoLibro.Saldo = bookDTO.Cantidad;
                    _context.MovimientoLibro.Update(movimientoLibro);
                }

                // Guardar los cambios
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Libro actualizado exitosamente.", BookId = book.BookId });
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que pueda ocurrir
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Error al actualizar el libro.",
                    Error = ex.Message
                });
            }
        }

    }
}
