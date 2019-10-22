using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Quotes")]
    public class Quote : DomainEntity
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public int? BookId { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; }

        public string Note { get; set; }
    }
}