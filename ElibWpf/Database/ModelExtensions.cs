using ElibWpf.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.DomainModel
{
    public partial class Book
    {
        DatabaseContext database = DatabaseContext.GetInstance();
        public override string ToString()
        {
            return $"ID: {id}  Name: {name} Authors: {string.Join(", ", database.GetBookAuthors(this).Select(x => x.name))}";
        }
    }

    public partial class Author
    {
        DatabaseContext database = DatabaseContext.GetInstance();
        public override string ToString()
        {
            return $"ID: {id}  Name: {name} Books: {string.Join(", ", database.GetAuthorBooks(this).Select(x => x.name))}";
        }
    }
}
