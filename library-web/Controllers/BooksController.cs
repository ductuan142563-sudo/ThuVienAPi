using library_web.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace library_web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:7165";

        public BooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ==================== INDEX (GET all) ====================
        public async Task<IActionResult> Index(string filterOn = null, string filterQuery = null,
                                               string sortBy = null, bool isAscending = true)
        {
            List<BookDTO> response = new();
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{_apiBaseUrl}/api/Books/get-all-books?filterOn={filterOn}&filterQuery={filterQuery}&sortBy={sortBy}&isAscending={isAscending}";

                var httpResponse = await client.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode();

                response.AddRange(await httpResponse.Content.ReadFromJsonAsync<IEnumerable<BookDTO>>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(response);
        }

        // ==================== DETAIL ====================
        public async Task<IActionResult> listBook(int id)
        {
            BookDTO response = new();
            try
            {
                var client = _httpClientFactory.CreateClient();
                var httpResponse = await client.GetAsync($"{_apiBaseUrl}/api/Books/get-book-by-id/{id}");
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadFromJsonAsync<BookDTO>();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(response);
        }

        // ==================== ADD (GET form) ====================
        [HttpGet]
        public async Task<IActionResult> addBook()
        {
            var client = _httpClientFactory.CreateClient();

            // Lấy danh sách Author
            var responseAu = await client.GetAsync($"{_apiBaseUrl}/api/Authors/get-all-author");
            responseAu.EnsureSuccessStatusCode();
            ViewBag.ListAuthor = await responseAu.Content.ReadFromJsonAsync<List<authorDTO>>();

            // Lấy danh sách Publisher
            var responsePu = await client.GetAsync($"{_apiBaseUrl}/api/Publishers/get-all-publisher");
            responsePu.EnsureSuccessStatusCode();
            ViewBag.ListPublisher = await responsePu.Content.ReadFromJsonAsync<List<publisherDTO>>();

            return View();
        }

        // ==================== ADD (POST) ====================
        [HttpPost]
        public async Task<IActionResult> addBook(addBookDTO addBookDTO)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{_apiBaseUrl}/api/Books/add-book"),
                    Content = new StringContent(
                        JsonSerializer.Serialize(addBookDTO),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(addBookDTO);
            }
        }

        // ==================== EDIT (GET form) ====================
        [HttpGet]
        public async Task<IActionResult> editBook(int id)
        {
            var client = _httpClientFactory.CreateClient();

            var httpResponse = await client.GetAsync($"{_apiBaseUrl}/api/Books/get-book-by-id/{id}");
            httpResponse.EnsureSuccessStatusCode();
            ViewBag.Book = await httpResponse.Content.ReadFromJsonAsync<BookDTO>();

            var responseAu = await client.GetAsync($"{_apiBaseUrl}/api/Authors/get-all-author");
            responseAu.EnsureSuccessStatusCode();
            ViewBag.ListAuthor = await responseAu.Content.ReadFromJsonAsync<List<authorDTO>>();

            var responsePu = await client.GetAsync($"{_apiBaseUrl}/api/Publishers/get-all-publisher");
            responsePu.EnsureSuccessStatusCode();
            ViewBag.ListPublisher = await responsePu.Content.ReadFromJsonAsync<List<publisherDTO>>();

            return View();
        }

        // ==================== EDIT (POST) ====================
        [HttpPost]
        public async Task<IActionResult> editBook(int id, editBookDTO bookDTO)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri($"{_apiBaseUrl}/api/Books/update-book-by-id/{id}"),
                    Content = new StringContent(
                        JsonSerializer.Serialize(bookDTO),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(bookDTO);
            }
        }

        // ==================== DELETE ====================
        [HttpGet]
        public async Task<IActionResult> delBook(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/Books/delete-book-by-id/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}