using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Quotes")]
    public class Quote
    {
        [ForeignKey("BookId")] public Book Book { get; set; }

        public int? BookId { get; set; }
        public int Id { get; set; }

        public string Note { get; set; }

        [Required] public string Text { get; set; }
    }
}