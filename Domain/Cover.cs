using MVVMLibrary;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Cover : ObservableObject
    {
        public int Id { get; set; }

        [Required] public byte[] Image { get; set; }
    }
}
