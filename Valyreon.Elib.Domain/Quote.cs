using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("Quotes")]
    public class Quote : ObservableEntity
    {
        private string text;
        private string note;

        [ForeignKey("BookId")] public Book Book { get; set; }

        [Column]
        public int? BookId { get; set; }

        [Required]
        [Column]
        public string Text
        {
            get => text;
            set => Set(() => Text, ref text, value);
        }

        [Column]
        public string Note
        {
            get => note;
            set => Set(() => Note, ref note, value);
        }
    }
}
