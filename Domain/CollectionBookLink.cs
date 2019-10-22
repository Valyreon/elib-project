using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class CollectionBookLink
    {
        public int Id { get; set; }

        [Required]
        public int CollectionId { get; set; }

        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; }

        [ForeignKey("CollectionId")]
        public UserCollection UserCollection { get; set; }
    }
}