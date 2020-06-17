using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class CollectionBookLink
    {
        public int Id { get; set; }

        [Required] public int BookId { get; set; }

        [Required] public int UserCollectionId { get; set; }
    }
}