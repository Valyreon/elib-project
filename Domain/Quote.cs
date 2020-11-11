using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MVVMLibrary;

namespace Domain
{
    [Table("Quotes")]
    public class Quote : ObservableObject
    {
        private string text;
        private string note;

        [ForeignKey("BookId")] public Book Book { get; set; }

        public int? BookId { get; set; }
        public int Id { get; set; }

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
