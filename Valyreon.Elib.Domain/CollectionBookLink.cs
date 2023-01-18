using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("UserCollectionBooks")]
    public class CollectionBookLink
    {
        public int Id { get; set; }

        [Required] public int BookId { get; set; }

        [Required] public int UserCollectionId { get; set; }
    }
}
