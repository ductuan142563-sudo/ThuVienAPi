using LTWebAPi.Data;
using LTWebAPi.Models.Image;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LTWebAPi.Repositories
{
    public class LocalImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public LocalImageRepository(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public Image Upload(Image image)
        {
            // 1. Tạo đường dẫn vật lý lưu file
            var localFilePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "Images",
                $"{image.FileName}{image.FileExtension}");

            // 2. Copy file vào folder Images
            using var stream = new FileStream(localFilePath, FileMode.Create);
            image.File.CopyTo(stream);

            // 3. Tạo URL public để client có thể truy cập
            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://" +
                              $"{_httpContextAccessor.HttpContext.Request.Host}" +
                              $"{_httpContextAccessor.HttpContext.Request.PathBase}/Images/" +
                              $"{image.FileName}{image.FileExtension}";

            image.FilePath = urlFilePath;

            // 4. Lưu metadata vào database
            _dbContext.Images.Add(image);
            _dbContext.SaveChanges();

            return image;
        }

        public List<Image> GetAllInfoImages()
        {
            return _dbContext.Images.ToList();
        }

        public (byte[], string, string) DownloadFile(int id)
        {
            var fileById = _dbContext.Images.FirstOrDefault(x => x.Id == id);

            if (fileById == null)
                throw new FileNotFoundException("Không tìm thấy file");

            var path = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "Images",
                $"{fileById.FileName}{fileById.FileExtension}");

            var stream = File.ReadAllBytes(path);
            var fileName = fileById.FileName + fileById.FileExtension;

            return (stream, "application/octet-stream", fileName);
        }
    }
}