using Biblioteca.DTOs;
using Biblioteca.Repositories;
using Biblioteca.Repositories.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _uploadFolder;

        public BookController(AppDbContext context, IConfiguration configuration, IWebHostEnvironment env)
        {
            _context = context;
            _configuration = configuration;
            if (env.ContentRootPath == null)
            {
                throw new ArgumentNullException(nameof(env.ContentRootPath), "El ContentRootPath no puede ser nulo.");
            }

            _uploadFolder = Path.Combine(env.ContentRootPath, "Imagenes");

            // Verificar si la carpeta de destino existe y crearla si no existe
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }


        [HttpPost("uploadbookimage")]
        public async Task<IActionResult> UploadBookImage([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "No se proporcionó ninguna imagen." });
            }

            try
            {
                var extension = Path.GetExtension(imageFile.FileName);
                var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + extension;
                var imagePath = Path.Combine(_uploadFolder, fileName);

                // Imprimir la ruta para depuración
                Console.WriteLine($"Saving image to: {imagePath}");

                // Guardar la imagen en esa carpeta
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return Ok(new { Success = true, FileName = fileName });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Error al cargar la imagen.",
                    Error = ex.Message
                });
            }
        }







        [HttpPost("addbookdata")]
        public async Task<IActionResult> AddBookData(BookDTO bookDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            try
            {
                var book = new Book
                {
                    BookId = Guid.NewGuid(),
                    Tittle = bookDTO.Tittle,
                    Author = bookDTO.Author,
                    Gender = bookDTO.Gender,
                    Year = bookDTO.Year,
                    Cantidad = bookDTO.Cantidad,
                    Imagen = bookDTO.Imagen // Aquí se guarda el alias en lugar del Base64
                };

                _context.Book.Add(book);
                await _context.SaveChangesAsync();

                // Crear y agregar un nuevo MovimientoLibro
                var movimientoLibro = new MovimientoLibro
                {
                    MovimientoLibroId = Guid.NewGuid(),
                    BookId = book.BookId,
                    Saldo = book.Cantidad // Saldo se iguala a Cantidad
                };

                _context.MovimientoLibro.Add(movimientoLibro);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Datos del libro registrados y movimiento creado.", BookId = book.BookId });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Error al agregar el libro.",
                    Error = ex.Message
                });
            }
        }


        [HttpPut("updatebookimage/{id}")]
        public async Task<IActionResult> UpdateBookImage(Guid id, [FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "No se proporcionó ninguna imagen." });
            }

            try
            {
                var book = await _context.Book.FindAsync(id);

                if (book == null)
                {
                    return NotFound(new { Success = false, Message = "Libro no encontrado." });
                }

                // Verificar si el libro ya tiene una imagen y eliminarla si es necesario
                if (!string.IsNullOrEmpty(book.Imagen))
                {
                    var oldImagePath = Path.Combine(_uploadFolder, book.Imagen);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Usar el nombre original de la imagen
                var originalFileName = Path.GetFileName(imageFile.FileName);
                var imagePath = Path.Combine(_uploadFolder, originalFileName);

                Console.WriteLine($"Saving image to: {imagePath}");

                // Guardar la nueva imagen en la carpeta
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Actualizar el libro con el nombre de la nueva imagen
                book.Imagen = originalFileName;

                _context.Book.Update(book);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Imagen del libro actualizada exitosamente.", ImageName = originalFileName });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Error al actualizar la imagen del libro.",
                    Error = ex.Message
                });
            }
        }



        [HttpPut("updatebookdata/{id}")]
        public async Task<IActionResult> UpdateBook(Guid id, BookDTO bookDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Datos no válidos.", Errors = ModelState.Values.SelectMany(v => v.Errors) });
            }

            try
            {
                var book = await _context.Book.FindAsync(id);

                if (book == null)
                {
                    return NotFound(new { Success = false, Message = "Libro no encontrado." });
                }

                // Actualizar los datos del libro
                book.Tittle = bookDTO.Tittle;
                book.Author = bookDTO.Author;
                book.Gender = bookDTO.Gender;
                book.Year = bookDTO.Year;
                book.Cantidad = bookDTO.Cantidad;

                // Si el alias de la imagen ha cambiado, se actualiza también
                if (!string.IsNullOrEmpty(bookDTO.Imagen))
                {
                    book.Imagen = bookDTO.Imagen;
                }

                _context.Book.Update(book);
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Datos del libro actualizados correctamente.", BookId = book.BookId });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Error al actualizar los datos del libro.",
                    Error = ex.Message
                });
            }
        }







        [HttpGet("bookimage/{fileName}")]
        public IActionResult GetBookImage(string fileName)
        {
            var filePath = Path.Combine(_uploadFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { Success = false, Message = "Imagen no encontrada." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            string contentType = fileExtension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };

            return File(fileBytes, contentType);
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
                    Imagen = u.Imagen 
                }).ToList();

                return Ok(new
                {
                    Success = true,
                    books = bookDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Se produjo un error al obtener los libros.",
                    Error = ex.Message
                });
            }
        }









    }
}
