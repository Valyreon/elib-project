using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Utilities
{
    public class DomainInitializer
    {
        public static Book EmptyBook()
        {
            return new Book()
            {
                Authors = new List<Author>(),
                Files = new List<EFile>(),
                Quotes = new List<Quote>(),
                UserCollections = new List<UserCollection>()
            };

        }

        public static Author EmptyAuthor()
        {
            return new Author()
            {
                Books = new List<Book>()
            };
        }

        public static Author NamedAuthor(string name)
        {
            Author result = EmptyAuthor();
            result.Name = name;
            return result;
        }

        public static UserCollection EmptyUserCollection()
        {
            return new UserCollection()
            {
                Books = new List<Book>()
            };
        }

        public static BookSeries EmptyBookSeries()
        {
            return new BookSeries()
            {
                Books = new List<Book>()
            };
        }

        public static BookSeries NamedBookSeries(string name)
        {
            BookSeries result = EmptyBookSeries();
            result.Name = name;
            return result;
        }
    }
}
