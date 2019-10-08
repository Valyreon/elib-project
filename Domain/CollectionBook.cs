using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class CollectionBook
    {
        public int CollectionId { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public UserCollection Collection { get; set; }
    }
}
