using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("Quotes")]
    public class Quote : ObservableEntity
    {
        private string note;
        private string text;
        [ForeignKey("BookId")] public Book Book { get; set; }

        [Column]
        public int? BookId { get; set; }

        [Column]
        public string Note
        {
            get => note;
            set => Set(() => Note, ref note, value);
        }

        [Required]
        [Column]
        public string Text
        {
            get => text;
            set => Set(() => Text, ref text, value);
        }
    }
}
