using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("AuthorBookLinks")]
    public class AuthorBookLink
    {
        public int Id { get; set; }
        [Required]
        public int BookId { get; set; }
        [Required]
        public int AuthorId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }
        [ForeignKey("AuthorId")]
        public Author Author { get; set; }
    }
}
