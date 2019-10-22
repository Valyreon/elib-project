using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("UserCollections")]
    public class UserCollection : DomainEntity
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Tag { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}