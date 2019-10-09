using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    [Table("CollectionBookLinks")]
    public class CollectionBookLink
    {
        public int Id { get; set; }
        [Required]
        public int CollectionId { get; set; }
        [Required]
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; }
        [ForeignKey("CollectionId")]
        public UserCollection Collection { get; set; }
    }
}
