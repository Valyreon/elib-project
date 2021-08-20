using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("UserCollections")]
    public class UserCollection : ObservableEntity
    {
        private string tag;

        public ICollection<Book> Books { get; set; }

        [Required]
        [StringLength(50)]
        public string Tag
        {
            get => tag;
            set => Set(() => Tag, ref tag, value);
        }
    }
}
