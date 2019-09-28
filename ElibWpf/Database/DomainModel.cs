using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.DomainModel
{
    [Table("Author")]
    public class Author
    {
        public long id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string metadata { get; set; }

        public virtual ICollection<book_author> book_authorValues { get; set; }
        public virtual ICollection<Series> SeriesValues { get; set; }
        public Author()
        {
            book_authorValues = new List<book_author>();
            SeriesValues = new List<Series>();
        }
    }

    [Table("Quote")]
    public class Quote
    {
        public long id { get; set; }
        public string text { get; set; }
        public string note { get; set; }
        public long bookId { get; set; }
        [ForeignKey("bookId")]
        public virtual Book book { get; set; }
    }

    [Table("File")]
    public class File
    {
        [Key, Column(Order = 0)]
        public long bookId { get; set; }
        [Key, Column(Order = 1)]
        public string format { get; set; }
        public byte[] fileBlob { get; set; }
        [ForeignKey("bookId")]
        public virtual Book book { get; set; }
    }

    [Table("Book")]
    public class Book
    {
        public long id { get; set; }
        public string name { get; set; }
        public int seriesNumber { get; set; }
        public byte[] cover { get; set; }
        public bool isRead { get; set; }
        public DateTime dateRead { get; set; }
        public long seriesId { get; set; }
        public string metadata { get; set; }

        public virtual ICollection<Quote> QuoteValues { get; set; }
        public virtual ICollection<File> FileValues { get; set; }
        public virtual ICollection<book_author> book_authorValues { get; set; }
        public virtual ICollection<collection_book> collection_bookValues { get; set; }
        public Book()
        {
            QuoteValues = new List<Quote>();
            FileValues = new List<File>();
            book_authorValues = new List<book_author>();
            collection_bookValues = new List<collection_book>();
        }
        [ForeignKey("seriesId")]
        public virtual Series series { get; set; }
    }

    [Table("Collection")]
    public class Collection
    {
        public long id { get; set; }
        public string name { get; set; }

        public virtual ICollection<collection_book> collection_bookValues { get; set; }
        public Collection()
        {
            collection_bookValues = new List<collection_book>();
        }
    }

    [Table("Series")]
    public class Series
    {
        public long id { get; set; }
        public string name { get; set; }
        
        public long authorId { get; set; }
        public string metadata { get; set; }
        public virtual ICollection<Book> BookValues { get; set; }
        public Series()
        {
            BookValues = new List<Book>();
        }

        [ForeignKey("authorId")]
        public virtual Author author { get; set; }


    }

    [Table("book_author")]
    public class book_author
    {
        [Key, Column(Order = 0)]
        public long book_id { get; set; }
        [Key, Column(Order = 1)]
        public long author_id { get; set; }

        [ForeignKey("author_id")]
        public virtual Author author { get; set; }
        [ForeignKey("book_id")]
        public virtual Book book { get; set; }
    }

    [Table("collection_book")]
    public class collection_book
    {
        [Key, Column(Order = 0)]
        public virtual long collection_id { get; set; }
        [Key, Column(Order = 1)]
        public virtual long book_id { get; set; }
        [ForeignKey("collection_id")]
        public virtual Collection collection { get; set; }
        [ForeignKey("book_id")]
        public virtual Book book { get; set; }
    }

}
