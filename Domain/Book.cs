using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public BookSeries Series { get; set; }
        public byte[] Cover { get; set; }
        public bool IsRead { get; set; }
        public DateTime WhenRead { get; set; }
        public ICollection<EFile> Files { get; set; }
        public ICollection<Author> Authors { get; set; }
        public ICollection<Quote> Quotes { get; set; }
        public ICollection<UserCollection> Collections { get; set; }
    }
}
