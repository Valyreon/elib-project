using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    [Table("Books")]
    public class Book
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public int SeriesId { get; set; }
        [ForeignKey("SeriesId")]
        public BookSeries Series { get; set; }
        public byte[] Cover { get; set; }
        public bool? IsRead { get; set; }
        public int? WhenRead { get; set; }
    }
}
