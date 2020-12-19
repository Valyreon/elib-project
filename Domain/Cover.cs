using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Cover : ObservableEntity
    {
        [Required] public byte[] Image { get; set; }
    }
}
