using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Books")]
    public class Book
    {
        public ICollection<Author> Authors { get; set; }
        public byte[] Cover { get; set; }

        [Required] [ForeignKey("FileId")] public EFile File { get; set; }

        public int FileId { get; set; }
        public int Id { get; set; }
        public bool IsFavorite { get; set; } = false;
        public bool IsRead { get; set; } = false;

        public decimal? NumberInSeries { get; set; }
        public decimal PercentageRead { get; set; } = 0;
        public ICollection<Quote> Quotes { get; set; }

        [ForeignKey("SeriesId")] public BookSeries Series { get; set; }

        public int? SeriesId { get; set; }

        [Required] [StringLength(100)] public string Title { get; set; }
        public ICollection<UserCollection> UserCollections { get; set; }
        public int? WhenRead { get; set; }
    }
}