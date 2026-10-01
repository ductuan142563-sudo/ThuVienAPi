using LTWebAPi.CustomActionFilters;
using LTWebAPi.Data;
using LTWebAPi.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BooksController> _logger;

        public BooksController(AppDbContext dbContext, IBookRepository bookRepository, ILogger<BooksController> logger)
        {
            _dbContext = dbContext;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        [HttpGet("get-all-books")]
        //[Authorize(Roles = "Read")]
        public IActionResult GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] bool isAscending,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 3)
        {
            _logger.LogInformation("GetAll Book Action method was invoked");

            // su dung reposity pattern
            var allBooks = _bookRepository.GetAllBooks(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

            _logger.LogInformation($"Finished GetAllBook request with data {System.Text.Json.JsonSerializer.Serialize(allBooks)}");

            return Ok(allBooks);
        }

        [HttpGet]
        [Route("get-book-by-id/{id}")]
        //[Authorize(Roles = "Read")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            _logger.LogInformation($"GetBookById Action method was invoked for id={id}");

            var bookWithIdDTO = _bookRepository.GetBookById(id);
            return Ok(bookWithIdDTO);
        }

        [HttpPost("add-book")]
        [ValidateModel]
        //[Authorize(Roles = "Write")]
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            _logger.LogInformation("AddBook Action method was invoked");

            if (ValidateAddBook(addBookRequestDTO))
            {
                var bookAdd = _bookRepository.AddBook(addBookRequestDTO);
                _logger.LogInformation($"Book added successfully: {System.Text.Json.JsonSerializer.Serialize(bookAdd)}");
                return Ok(bookAdd);
            }

            _logger.LogWarning("AddBook validation failed");
            return BadRequest(ModelState);
        }

        [HttpPut("update-book-by-id/{id}")]
        //[Authorize(Roles = "Write")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            _logger.LogInformation($"UpdateBookById Action method was invoked for id={id}");
            var updateBook = _bookRepository.UpdateBookById(id, bookDTO);
            return Ok(updateBook);
        }

        [HttpDelete("delete-book-by-id/{id}")]
        //[Authorize(Roles = "Write")]
        public IActionResult DeleteBookById(int id)
        {
            _logger.LogInformation($"DeleteBookById Action method was invoked for id={id}");

            var deleteBook = _bookRepository.DeleteBookById(id);
            return Ok(deleteBook);
        }

        #region Private methods
        private bool ValidateAddBook(AddBookRequestDTO addBookRequestDTO)
        {
            if (addBookRequestDTO == null)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO), $"Please add book data");
                return false;
            }
            // kiem tra Description NotNull
            if (string.IsNullOrEmpty(addBookRequestDTO.Description))
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Description),
                    $"{nameof(addBookRequestDTO.Description)} cannot be null");
            }
            // kiem tra rating (0,5)
            if (addBookRequestDTO.Rate < 0 || addBookRequestDTO.Rate > 5)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Rate),
                    $"{nameof(addBookRequestDTO.Rate)} cannot be less than 0 and more than 5");
            }

            if (ModelState.ErrorCount > 0)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}