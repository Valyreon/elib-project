using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Quote
    {
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        public Book Book { get; set; }
        public string Note { get; set; }
    }
}