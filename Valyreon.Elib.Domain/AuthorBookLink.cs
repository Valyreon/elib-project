using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("AuthorBooks")]
    public class AuthorBookLink
    {
        [Required] public int AuthorId { get; set; }
        [Required] public int BookId { get; set; }
        public int Id { get; set; }
    }
}
