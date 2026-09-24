using LTWebAPi.Data;
using LTWebAPi.Models.Domain;
using LTWebAPi.Models.DTO;

namespace LTWebAPi.Repositories
{
    public class SQLAuthorRepository : IAuthorRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLAuthorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. Lấy tất cả Author
        public List<AuthorDTO> GetAllAuthors()
        {
            var allAuthorsDomain = _dbContext.Authors.ToList();

            var allAuthorDTO = new List<AuthorDTO>();

            foreach (var authorDomain in allAuthorsDomain)
            {
                allAuthorDTO.Add(new AuthorDTO
                {
                    Id = authorDomain.Id,
                    FullName = authorDomain.FullName
                });
            }

            return allAuthorDTO;
        }

        // 2. Lấy Author theo Id
        public AuthorNoIdDTO GetAuthorById(int id)
        {
            var authorWithIdDomain = _dbContext.Authors.FirstOrDefault(x => x.Id == id);

            if (authorWithIdDomain == null)
            {
                return null;
            }

            return new AuthorNoIdDTO
            {
                FullName = authorWithIdDomain.FullName
            };
        }

        // 3. Thêm Author mới
        public AddAuthorRequestDTO AddAuthor(AddAuthorRequestDTO addAuthorRequestDTO)
        {
            var authorDomainModel = new Author
            {
                FullName = addAuthorRequestDTO.FullName
            };

            _dbContext.Authors.Add(authorDomainModel);
            _dbContext.SaveChanges();

            return addAuthorRequestDTO;
        }

        // 4. Cập nhật Author
        public AuthorNoIdDTO UpdateAuthorById(int id, AuthorNoIdDTO authorNoIdDTO)
        {
            var authorDomain = _dbContext.Authors.FirstOrDefault(n => n.Id == id);

            if (authorDomain != null)
            {
                authorDomain.FullName = authorNoIdDTO.FullName;
                _dbContext.SaveChanges();
            }

            return authorNoIdDTO;
        }

        // 5. Xóa Author
        public Author? DeleteAuthorById(int id)
        {
            var authorDomain = _dbContext.Authors.FirstOrDefault(n => n.Id == id);

            if (authorDomain != null)
            {
                _dbContext.Authors.Remove(authorDomain);
                _dbContext.SaveChanges();
            }

            return authorDomain;
        }
    }
}