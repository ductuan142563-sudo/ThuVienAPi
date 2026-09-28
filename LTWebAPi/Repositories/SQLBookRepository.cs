using LTWebAPi.Data;
using LTWebAPi.Models.Domain;
using LTWebAPi.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace LTWebAPi.Repositories
{
    public class SQLBookRepository : IBookRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLBookRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ==================== GET ALL (Filter + Sort + Pagination) ====================
        public List<BookWithAuthorAndPublisherDTO> GetAllBooks(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 1000)
        {
            var allBooks = _dbContext.Books
                .Select(b => new BookWithAuthorAndPublisherDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    IsRead = b.IsRead,
                    DateRead = b.IsRead ? b.DateRead : null,
                    Rate = b.IsRead ? b.Rate : null,
                    Genre = b.Genre,
                    CoverUrl = b.CoverUrl,
                    PublisherName = b.Publisher.Name,
                    AuthorNames = b.Book_Authors.Select(n => n.Author.FullName).ToList()
                })
                .AsQueryable();

            // ========== FILTER ==========
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = allBooks.Where(x => x.Title.Contains(filterQuery));
                }
                else if (filterOn.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = allBooks.Where(x =>
                        x.Description != null &&
                        x.Description.Contains(filterQuery));
                }
                else if (filterOn.Equals("genre", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = allBooks.Where(x =>
                        x.Genre != null &&
                        x.Genre.Contains(filterQuery));
                }
                else if (filterOn.Equals("rate", StringComparison.OrdinalIgnoreCase)
                         && int.TryParse(filterQuery, out int rateValue))
                {
                    allBooks = allBooks.Where(x => x.Rate == rateValue);
                }
                else if (filterOn.Equals("isread", StringComparison.OrdinalIgnoreCase)
                         && bool.TryParse(filterQuery, out bool isReadValue))
                {
                    allBooks = allBooks.Where(x => x.IsRead == isReadValue);
                }
            }

            // ========== SORT ==========
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = isAscending
                        ? allBooks.OrderBy(x => x.Title)
                        : allBooks.OrderByDescending(x => x.Title);
                }
                else if (sortBy.Equals("rate", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = isAscending
                        ? allBooks.OrderBy(x => x.Rate)
                        : allBooks.OrderByDescending(x => x.Rate);
                }
                else if (sortBy.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    allBooks = isAscending
                        ? allBooks.OrderBy(x => x.Id)
                        : allBooks.OrderByDescending(x => x.Id);
                }
            }

            // ========== PAGINATION ==========
            var skipResults = (pageNumber - 1) * pageSize;
            return allBooks.Skip(skipResults).Take(pageSize).ToList();
        }

        // ==================== GET BY ID ====================
        public BookWithAuthorAndPublisherDTO GetBookById(int id)
        {
            var book = _dbContext.Books
                .Where(b => b.Id == id)
                .Select(b => new BookWithAuthorAndPublisherDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    IsRead = b.IsRead,
                    DateRead = b.IsRead ? b.DateRead : null,
                    Rate = b.IsRead ? b.Rate : null,
                    Genre = b.Genre,
                    CoverUrl = b.CoverUrl,
                    PublisherName = b.Publisher.Name,
                    AuthorNames = b.Book_Authors.Select(n => n.Author.FullName).ToList()
                })
                .FirstOrDefault();

            return book;
        }

        // ==================== ADD BOOK ====================
        public AddBookRequestDTO AddBook(AddBookRequestDTO addBookRequestDTO)
        {
            var bookDomain = new Book
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                PublisherId = addBookRequestDTO.PublisherID
            };

            _dbContext.Books.Add(bookDomain);
            _dbContext.SaveChanges();

            // Thêm quan hệ Book_Author nếu có
            if (addBookRequestDTO.AuthorIds != null && addBookRequestDTO.AuthorIds.Any())
            {
                foreach (var authorId in addBookRequestDTO.AuthorIds)
                {
                    var bookAuthor = new Book_Author
                    {
                        BookId = bookDomain.Id,
                        AuthorId = authorId
                    };
                    _dbContext.Books_Authors.Add(bookAuthor);
                }
                _dbContext.SaveChanges();
            }

            return addBookRequestDTO;
        }

        // ==================== UPDATE BOOK ====================
        public AddBookRequestDTO? UpdateBookById(int id, AddBookRequestDTO bookDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(b => b.Id == id);
            if (bookDomain == null) return null;

            bookDomain.Title = bookDTO.Title;
            bookDomain.Description = bookDTO.Description;
            bookDomain.IsRead = bookDTO.IsRead;
            bookDomain.DateRead = bookDTO.DateRead;
            bookDomain.Rate = bookDTO.Rate;
            bookDomain.Genre = bookDTO.Genre;
            bookDomain.CoverUrl = bookDTO.CoverUrl;
            bookDomain.PublisherId = bookDTO.PublisherID;

            // Cập nhật quan hệ Author (xóa cũ + thêm mới)
            var existingAuthors = _dbContext.Books_Authors
                .Where(ba => ba.BookId == id)
                .ToList();

            _dbContext.Books_Authors.RemoveRange(existingAuthors);

            if (bookDTO.AuthorIds != null && bookDTO.AuthorIds.Any())
            {
                foreach (var authorId in bookDTO.AuthorIds)
                {
                    _dbContext.Books_Authors.Add(new Book_Author
                    {
                        BookId = id,
                        AuthorId = authorId
                    });
                }
            }

            _dbContext.SaveChanges();
            return bookDTO;
        }

        // ==================== DELETE BOOK ====================
        public Book? DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(b => b.Id == id);
            if (bookDomain == null) return null;

            // Xóa quan hệ Book_Author trước
            var bookAuthors = _dbContext.Books_Authors
                .Where(ba => ba.BookId == id)
                .ToList();

            _dbContext.Books_Authors.RemoveRange(bookAuthors);
            _dbContext.Books.Remove(bookDomain);
            _dbContext.SaveChanges();

            return bookDomain;
        }

        // ==================== VALIDATE HELPERS ====================
        public bool PublisherExists(int publisherId)
        {
            return _dbContext.Publishers.Any(p => p.Id == publisherId);
        }

        public bool IsTitleDuplicateInPublisher(string title, int publisherId)
        {
            return _dbContext.Books.Any(b =>
                b.Title.ToLower() == title.ToLower() &&
                b.PublisherId == publisherId);
        }

        public bool AuthorExists(int authorId)
        {
            return _dbContext.Authors.Any(a => a.Id == authorId);
        }

        public bool BookExists(int bookId)
        {
            return _dbContext.Books.Any(b => b.Id == bookId);
        }

        public bool BookAuthorRelationExists(int bookId, int authorId)
        {
            return _dbContext.Books_Authors.Any(ba =>
                ba.BookId == bookId && ba.AuthorId == authorId);
        }
    }
}