using LTWebAPi.Models.Image;
using System.Collections.Generic;

namespace LTWebAPi.Repositories
{
    public interface IImageRepository
    {
        Image Upload(Image image);
        List<Image> GetAllInfoImages();
        (byte[], string, string) DownloadFile(int id);
    }
}