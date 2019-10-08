using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Author
    {
        public int Id { get; set; }
        [Required]
        public int Name { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
