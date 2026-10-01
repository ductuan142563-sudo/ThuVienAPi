using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LTWebAPi.Data
{
    public class BookAuthDbContextFactory : IDesignTimeDbContextFactory<BookAuthDbContext>
    {
        public BookAuthDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookAuthDbContext>();

            // Thay connection string cho đúng với máy bạn
            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS;Database=WebAPI_Books;Trusted_Connection=True;TrustServerCertificate=True;");

            return new BookAuthDbContext(optionsBuilder.Options);
        }
    }
}