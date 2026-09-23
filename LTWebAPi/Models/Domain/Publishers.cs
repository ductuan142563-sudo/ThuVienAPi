using LTWebAPi.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace LTWebAPi.Models.Domain
{
    public class Publisher
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation Properties
        public List<Book> Books { get; set; }
    }
}