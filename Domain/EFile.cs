using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("EBookFiles")]
    public class EFile : ObservableEntity
    {
        [Required] [StringLength(10)] public string Format { get; set; }

        [Required] public RawFile RawFile { get; set; }

        public int RawFileId { get; set; }

        [Required] public string Signature { get; set; }
    }
}
