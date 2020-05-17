using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("EBookFiles")]
    public class EFile
    {
        [Required] [StringLength(10)] public string Format { get; set; }
        public int Id { get; set; }

        [Required] [ForeignKey("RawFileId")] public RawFile RawFile { get; set; }

        public int RawFileId { get; set; }

        [Required] public string Signature { get; set; }
    }
}