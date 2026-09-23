using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTWebAPi.Data;
using LTWebAPi.Models.Domain;
using LTWebAPi.Models.DTO;

namespace LTWebAPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public BooksController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ====================== GET ALL ======================
        [HttpGet("get-all-books")]
        public IActionResult GetAll()
        {
            var allBooksDomain = _dbContext.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                    .ThenInclude(ba => ba.Author)
                .ToList();

            var allBooksDTO = allBooksDomain.Select(book => new BookWithAuthorAndPublisherDTO
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.IsRead ? book.DateRead : null,
                Rate = book.IsRead ? book.Rate : null,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = book.DateAdded,
                PublisherName = book.Publisher?.Name ?? "Unknown",
                AuthorNames = book.Book_Authors?
                    .Where(ba => ba.Author != null)
                    .Select(ba => ba.Author.FullName)
                    .ToList() ?? new List<string>()
            }).ToList();

            return Ok(allBooksDTO);
        }

        // ====================== GET BY ID ======================
        [HttpGet("get-book-by-id/{id:int}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookDomain = _dbContext.Books
                .Include(b => b.Publisher)
                .Include(b => b.Book_Authors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefault(b => b.Id == id);

            if (bookDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }

            var bookDTO = new BookWithAuthorAndPublisherDTO
            {
                Id = bookDomain.Id,
                Title = bookDomain.Title,
                Description = bookDomain.Description,
                IsRead = bookDomain.IsRead,
                DateRead = bookDomain.DateRead,
                Rate = bookDomain.Rate,
                Genre = bookDomain.Genre,
                CoverUrl = bookDomain.CoverUrl,
                DateAdded = bookDomain.DateAdded,
                PublisherName = bookDomain.Publisher?.Name ?? "Unknown",
                AuthorNames = bookDomain.Book_Authors?
                    .Where(ba => ba.Author != null)
                    .Select(ba => ba.Author.FullName)
                    .ToList() ?? new List<string>()
            };

            return Ok(bookDTO);
        }

        // ====================== ADD BOOK ======================
        [HttpPost("add-book")]
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            // Kiểm tra Publisher có tồn tại không
            var publisherDomain = _dbContext.Publishers
                .FirstOrDefault(x => x.Id == addBookRequestDTO.PublisherID);

            if (publisherDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy Nhà xuất bản" });
            }

            // Tạo Book mới
            var bookDomain = new Book
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherId = publisherDomain.Id
            };

            _dbContext.Books.Add(bookDomain);
            _dbContext.SaveChanges();

            // Thêm các Author vào bảng trung gian
            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                var authorDomain = _dbContext.Authors.FirstOrDefault(x => x.Id == authorId);
                if (authorDomain == null)
                {
                    return NotFound(new { message = $"Không tìm thấy tác giả ID = {authorId}" });
                }

                var bookAuthor = new Book_Author
                {
                    BookId = bookDomain.Id,
                    AuthorId = authorDomain.Id
                };

                _dbContext.Books_Authors.Add(bookAuthor);
            }

            _dbContext.SaveChanges();

            return Ok(new { message = "Thêm sách thành công", bookId = bookDomain.Id });
        }

        // ====================== UPDATE BOOK ======================
        [HttpPut("update-book-by-id/{id:int}")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(x => x.Id == id);

            if (bookDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }

            // Cập nhật thông tin sách
            bookDomain.Title = addBookRequestDTO.Title;
            bookDomain.Description = addBookRequestDTO.Description;
            bookDomain.IsRead = addBookRequestDTO.IsRead;
            bookDomain.DateRead = addBookRequestDTO.DateRead;
            bookDomain.Rate = addBookRequestDTO.Rate;
            bookDomain.Genre = addBookRequestDTO.Genre;
            bookDomain.CoverUrl = addBookRequestDTO.CoverUrl;
            bookDomain.DateAdded = addBookRequestDTO.DateAdded;
            bookDomain.PublisherId = addBookRequestDTO.PublisherID;

            _dbContext.SaveChanges();

            // Xóa các quan hệ Author cũ
            var existingBookAuthors = _dbContext.Books_Authors
                .Where(x => x.BookId == id)
                .ToList();

            if (existingBookAuthors.Any())
            {
                _dbContext.Books_Authors.RemoveRange(existingBookAuthors);
                _dbContext.SaveChanges();
            }

            // Thêm lại các Author mới
            foreach (var authorId in addBookRequestDTO.AuthorIds)
            {
                var authorDomain = _dbContext.Authors.FirstOrDefault(x => x.Id == authorId);
                if (authorDomain == null)
                {
                    return NotFound(new { message = $"Không tìm thấy tác giả ID = {authorId}" });
                }

                var bookAuthor = new Book_Author
                {
                    BookId = bookDomain.Id,
                    AuthorId = authorDomain.Id
                };

                _dbContext.Books_Authors.Add(bookAuthor);
            }

            _dbContext.SaveChanges();

            return Ok(new { message = "Cập nhật sách thành công" });
        }

        // ====================== DELETE BOOK ======================
        [HttpDelete("delete-book-by-id/{id:int}")]
        public IActionResult DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(x => x.Id == id);

            if (bookDomain == null)
            {
                return NotFound(new { message = "Không tìm thấy sách" });
            }

            // Xóa các quan hệ trong bảng trung gian trước
            var existingBookAuthors = _dbContext.Books_Authors
                .Where(x => x.BookId == id)
                .ToList();

            if (existingBookAuthors.Any())
            {
                _dbContext.Books_Authors.RemoveRange(existingBookAuthors);
                _dbContext.SaveChanges();
            }

            // Xóa sách
            _dbContext.Books.Remove(bookDomain);
            _dbContext.SaveChanges();

            return Ok(new { message = "Xóa sách thành công" });
        }
    }
}