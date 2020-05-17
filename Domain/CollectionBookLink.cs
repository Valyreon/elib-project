using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class CollectionBookLink
    {
        [ForeignKey("BookId")] public Book Book { get; set; }

        [Required] public int BookId { get; set; }

        [Required] public int CollectionId { get; set; }
        public int Id { get; set; }

        [ForeignKey("CollectionId")] public UserCollection UserCollection { get; set; }
    }
}