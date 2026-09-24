using LTWebAPi.Data;
using LTWebAPi.Models.Domain;
using LTWebAPi.Models.DTO;
using LTWebAPi.Repositories;

namespace LTWebAPi.Repositories
{
    public class SQLPublisherRepository : IPublisherRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLPublisherRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. Lấy tất cả Publisher
        public List<PublisherDTO> GetAllPublishers()
        {
            var allPublishersDomain = _dbContext.Publishers.ToList();

            var allPublisherDTO = new List<PublisherDTO>();

            foreach (var publisherDomain in allPublishersDomain)
            {
                allPublisherDTO.Add(new PublisherDTO
                {
                    Id = publisherDomain.Id,
                    Name = publisherDomain.Name
                });
            }

            return allPublisherDTO;
        }

        // 2. Lấy Publisher theo Id
        public PublisherNoIdDTO GetPublisherById(int id)
        {
            var publisherWithIdDomain = _dbContext.Publishers.FirstOrDefault(x => x.Id == id);

            if (publisherWithIdDomain == null)
            {
                return null;
            }

            return new PublisherNoIdDTO
            {
                Name = publisherWithIdDomain.Name
            };
        }

        // 3. Thêm Publisher mới
        public AddPublisherRequestDTO AddPublisher(AddPublisherRequestDTO addPublisherRequestDTO)
        {
            var publisherDomainModel = new Publisher
            {
                Name = addPublisherRequestDTO.Name
            };

            _dbContext.Publishers.Add(publisherDomainModel);
            _dbContext.SaveChanges();

            return addPublisherRequestDTO;
        }

        // 4. Cập nhật Publisher
        public PublisherNoIdDTO UpdatePublisherById(int id, PublisherNoIdDTO publisherNoIdDTO)
        {
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);

            if (publisherDomain != null)
            {
                publisherDomain.Name = publisherNoIdDTO.Name;
                _dbContext.SaveChanges();
            }

            return publisherNoIdDTO;
        }

        // 5. Xóa Publisher
        public Publisher? DeletePublisherById(int id)
        {
            var publisherDomain = _dbContext.Publishers.FirstOrDefault(n => n.Id == id);

            if (publisherDomain != null)
            {
                _dbContext.Publishers.Remove(publisherDomain);
                _dbContext.SaveChanges();
            }

            return publisherDomain;
        }
    }
}