using LTWebAPi.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace LTWebAPi.Models.Domain
{
    public class Author
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }

        // Navigation Properties
        public List<Book_Author> Book_Authors { get; set; }
    }
}