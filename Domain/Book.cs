using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Books")]
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public int? SeriesId { get; set; }

        [ForeignKey("SeriesId")]
        public BookSeries Series { get; set; }

        public decimal? NumberInSeries { get; set; }
        public byte[] Cover { get; set; }
        public bool IsRead { get; set; } = false;
        public int? WhenRead { get; set; }
        public bool IsFavorite { get; set; } = false;
        public decimal PercentageRead { get; set; } = 0;
        public ICollection<Author> Authors { get; set; }
        public ICollection<EFile> Files { get; set; }
        public ICollection<Quote> Quotes { get; set; }
        public ICollection<UserCollection> UserCollections { get; set; }

        public string SeriesInfo
        {
            get => Series != null ? $"{Series.Name} {((NumberInSeries!=null)?($"#{NumberInSeries}"):(""))}" : "";
        }
    }
}