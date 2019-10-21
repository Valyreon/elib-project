using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("BookFiles")]
    public class EFile
    {
        public int Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Format { get; set; }
        [Required]
        public byte[] RawContent { get; set; }
        public int BookId { get; set; }
        [Required]
        [ForeignKey("BookId")]
        public Book Book { get; set; }
    }
}