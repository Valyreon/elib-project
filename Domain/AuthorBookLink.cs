using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class AuthorBookLink
    {
        public int Id { get; set; }

        [Required] public int AuthorId { get; set; }

        [Required] public int BookId { get; set; }
    }
}