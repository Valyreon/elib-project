using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("RawFiles")]
    public class RawFile : ObservableEntity
    {
        [Required]
        [Column]
        public byte[] RawContent { get; set; }
    }
}
