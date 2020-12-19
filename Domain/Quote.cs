using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Quotes")]
    public class Quote : ObservableEntity
    {
        private string text;
        private string note;

        [ForeignKey("BookId")] public Book Book { get; set; }

        public int? BookId { get; set; }

        [Required]
        public string Text
        {
            get => text;
            set => Set(() => Text, ref text, value);
        }

        public string Note
        {
            get => note;
            set => Set(() => Note, ref note, value);
        }
    }
}
