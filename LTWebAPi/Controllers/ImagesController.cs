using LTWebAPi.Models.DTO;
using LTWebAPi.Models.Image;         
using Microsoft.AspNetCore.Mvc;
using LTWebAPi.Repositories;

namespace LTWebAPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;

        public ImagesController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        // POST: /api/Images/Upload
        [HttpPost]
        [Route("Upload")]
        public IActionResult Upload([FromForm] ImageUploadRequestDTO request)
        {
            ValidateFileUpload(request);

            if (ModelState.IsValid)
            {
                // Convert DTO → Domain Model
                var imageDomainModel = new Image
                {
                    File = request.File,
                    FileExtension = Path.GetExtension(request.File.FileName).ToLower(),
                    FileSizeInBytes = request.File.Length,
                    FileName = request.FileName,
                    FileDescription = request.FileDescription
                };

                // Gọi Repository để upload
                _imageRepository.Upload(imageDomainModel);

                return Ok(imageDomainModel);
            }

            return BadRequest(ModelState);
        }

        // GET: /api/Images
        [HttpGet]
        public IActionResult GetAllInfoImages()
        {
            var allImages = _imageRepository.GetAllInfoImages();
            return Ok(allImages);
        }

        // GET: /api/Images/Download?id=1
        [HttpGet]
        [Route("Download")]
        public IActionResult DownloadImage([FromQuery] int id)
        {
            var result = _imageRepository.DownloadFile(id);
            return File(result.Item1, result.Item2, result.Item3);
        }

        // ========== Validation ==========
        private void ValidateFileUpload(ImageUploadRequestDTO request)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName).ToLower()))
            {
                ModelState.AddModelError("file", "Unsupported file extension. Chỉ chấp nhận .jpg, .jpeg, .png");
            }

            // Giới hạn 10MB
            if (request.File.Length > 10485760)
            {
                ModelState.AddModelError("file", "File size too big, please upload file < 10MB");
            }
        }
    }
}