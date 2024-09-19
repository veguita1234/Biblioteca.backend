﻿using Biblioteca.DTOs;
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

            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }


        //[HttpPost("uploadbookimage")]
        //public async Task<IActionResult> UploadBookImage([FromForm] IFormFile imageFile)
        //{
        //    if (imageFile == null || imageFile.Length == 0)
        //    {
        //        return BadRequest(new { Success = false, Message = "No se proporcionó ninguna imagen." });
        //    }

        //    try
        //    {
        //        var extension = Path.GetExtension(imageFile.FileName);
        //        var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + extension;
        //        var imagePath = Path.Combine(_uploadFolder, fileName);


        //        using (var stream = new FileStream(imagePath, FileMode.Create))
        //        {
        //            await imageFile.CopyToAsync(stream);
        //        }

        //        return Ok(new { Success = true, FileName = fileName });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new
        //        {
        //            Success = false,
        //            Message = "Error al cargar la imagen.",
        //            Error = ex.Message
        //        });
        //    }
        //}


        [HttpPost("uploadbookimage")]
        public async Task<IActionResult> UploadBookImage([FromForm] BookImageDTO bookImageDTO)
        {
            // Validar que solo uno de los dos campos esté presente, pero no ambos.
            if (bookImageDTO.ImageFile != null && !string.IsNullOrEmpty(bookImageDTO.ImageUrl))
            {
                return BadRequest(new { Success = false, Message = "Solo puede proporcionar un archivo o una URL, no ambos." });
            }

            // Validar que al menos uno de los dos esté presente.
            if (bookImageDTO.ImageFile == null && string.IsNullOrEmpty(bookImageDTO.ImageUrl))
            {
                return BadRequest(new { Success = false, Message = "Debe proporcionar una imagen o una URL." });
            }

            try
            {
                string fileName = string.Empty;

                // Si se proporciona un archivo, lo guardamos localmente
                if (bookImageDTO.ImageFile != null)
                {
                    var extension = Path.GetExtension(bookImageDTO.ImageFile.FileName);
                    fileName = Path.GetFileNameWithoutExtension(bookImageDTO.ImageFile.FileName) + extension;
                    var imagePath = Path.Combine(_uploadFolder, fileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await bookImageDTO.ImageFile.CopyToAsync(stream);
                    }
                }
                // Si se proporciona una URL, la descargamos y la guardamos localmente
                else if (!string.IsNullOrEmpty(bookImageDTO.ImageUrl))
                {
                    var uri = new Uri(bookImageDTO.ImageUrl);
                    fileName = Path.GetFileName(uri.LocalPath);
                    var imagePath = Path.Combine(_uploadFolder, fileName);

                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(uri);

                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest(new { Success = false, Message = "No se pudo descargar la imagen desde la URL proporcionada." });
                        }

                        var imageBytes = await response.Content.ReadAsByteArrayAsync();
                        await System.IO.File.WriteAllBytesAsync(imagePath, imageBytes);
                    }
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
                    Imagen = bookDTO.Imagen 
                };

                _context.Book.Add(book);
                await _context.SaveChangesAsync();

                var movimientoLibro = new MovimientoLibro
                {
                    MovimientoLibroId = Guid.NewGuid(),
                    BookId = book.BookId,
                    Saldo = book.Cantidad 
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

                if (!string.IsNullOrEmpty(book.Imagen))
                {
                    var oldImagePath = Path.Combine(_uploadFolder, book.Imagen);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var originalFileName = Path.GetFileName(imageFile.FileName);
                var imagePath = Path.Combine(_uploadFolder, originalFileName);

                Console.WriteLine($"Saving image to: {imagePath}");

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

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

                book.Tittle = bookDTO.Tittle;
                book.Author = bookDTO.Author;
                book.Gender = bookDTO.Gender;
                book.Year = bookDTO.Year;
                book.Cantidad = bookDTO.Cantidad;

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



        private string GetFileNameFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // Extrae el nombre del archivo de la URL
            return url.Substring(url.LastIndexOf('/') + 1);
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
                    Imagen = GetFileNameFromUrl(u.Imagen)
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

        [HttpGet("book/{id}")]
        public async Task<IActionResult> GetBookById(Guid id)
        {
            try
            {
                var book = await _context.Book.FindAsync(id);

                if (book == null)
                {
                    return NotFound(new { Success = false, Message = "Libro no encontrado." });
                }

                var bookDto = new BookDTO
                {
                    BookId = book.BookId,
                    Tittle = book.Tittle,
                    Author = book.Author,
                    Gender = book.Gender,
                    Year = book.Year,
                    Cantidad = book.Cantidad,
                    Imagen = book.Imagen
                };

                return Ok(new { Success = true, Book = bookDto });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Se produjo un error al obtener el libro.",
                    Error = ex.Message
                });
            }


        }

        [HttpGet("books/filter")]
        public async Task<IActionResult> GetBooksByFilter([FromQuery] string searchTerm)
        {
            try
            {
                var query = _context.Book.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Convierte el término de búsqueda a minúsculas para la comparación
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(b =>
                        b.Tittle.ToLower().Contains(searchTerm) ||
                        (b.Author != null && b.Author.ToLower().Contains(searchTerm)) ||
                        b.Gender.ToLower().Contains(searchTerm)
                    );
                }

                var books = await query.ToListAsync();

                var bookDtos = books.Select(b => new BookDTO
                {
                    BookId = b.BookId,
                    Tittle = b.Tittle,
                    Author = b.Author,
                    Gender = b.Gender,
                    Year = b.Year,
                    Cantidad = b.Cantidad,
                    Imagen = b.Imagen
                }).ToList();

                return Ok(new
                {
                    Success = true,
                    Books = bookDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Success = false,
                    Message = "Se produjo un error al obtener los libros filtrados.",
                    Error = ex.Message
                });
            }
        }








    }
}
