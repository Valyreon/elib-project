using System.ComponentModel.DataAnnotations;

namespace Valyreon.Elib.Domain
{
    public class CollectionBookLink
    {
        public int Id { get; set; }

        [Required] public int BookId { get; set; }

        [Required] public int UserCollectionId { get; set; }
    }
}
