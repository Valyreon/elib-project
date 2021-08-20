using System.ComponentModel.DataAnnotations;

namespace Valyreon.Elib.Domain
{
    public class Cover : ObservableEntity
    {
        [Required] public byte[] Image { get; set; }
    }
}
