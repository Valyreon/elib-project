using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class AuthorBookLink
    {
        [ForeignKey("AuthorId")] public Author Author { get; set; }

        [Required] public int AuthorId { get; set; }

        [ForeignKey("BookId")] public Book Book { get; set; }

        [Required] public int BookId { get; set; }
        public int Id { get; set; }
    }
}