using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("Covers")]
    public class Cover : ObservableEntity
    {
        [Required]
        [Column]
        public byte[] Image { get; set; }
    }
}
