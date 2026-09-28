using LTWebAPi.Models.Domain;
using LTWebAPi.Models.DTO;

namespace LTWebAPi.Repositories
{
    public interface IBookRepository
    {
        // ===== GET ALL (Filter + Sort + Pagination) =====
        List<BookWithAuthorAndPublisherDTO> GetAllBooks(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 1000);

        // ===== GET BY ID =====
        BookWithAuthorAndPublisherDTO GetBookById(int id);

        // ===== ADD =====
        AddBookRequestDTO AddBook(AddBookRequestDTO addBookRequestDTO);

        // ===== UPDATE =====
        AddBookRequestDTO? UpdateBookById(int id, AddBookRequestDTO bookDTO);

        // ===== DELETE =====
        Book? DeleteBookById(int id);

        // ===== VALIDATE HELPERS =====
        bool PublisherExists(int publisherId);
        bool IsTitleDuplicateInPublisher(string title, int publisherId);
        bool AuthorExists(int authorId);
        bool BookExists(int bookId);
        bool BookAuthorRelationExists(int bookId, int authorId);
    }
}