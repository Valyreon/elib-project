using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("RawFiles")]
    public class RawFile
    {
        public int Id { get; set; }

        [Required] public byte[] RawContent { get; set; }
    }
}