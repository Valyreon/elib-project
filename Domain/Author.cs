using MVVMLibrary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Authors")]
    public class Author : ObservableObject
    {
        private string name;

        public IEnumerable<Book> Books { get; set; }
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => this.name;
            set => Set(() => Name, ref name, value);
        }
    }
}