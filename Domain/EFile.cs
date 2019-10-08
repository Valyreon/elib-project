using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class EFile
    {
        public int Id { get; set; }
        [Required]
        public string Format { get; set; }
        [Required]
        public byte[] RawContent { get; set; }
        [Required]
        public Book Book { get; set; }
    }
}