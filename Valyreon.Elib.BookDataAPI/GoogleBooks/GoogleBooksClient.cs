using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Valyreon.Elib.BookDataAPI.GoogleBooks.Dtos;

namespace Valyreon.Elib.BookDataAPI.GoogleBooks;

public class GoogleBooksClient : IBookInformationAPI
{
    private readonly string _baseUrl = "https://www.googleapis.com/books/v1";
    private readonly bool throttle;
    private DateTime? lastRequestTime;

    public GoogleBooksClient(bool throttle = true)
    {
        this.throttle = throttle;
    }

    public async Task<BookInformation> GetByIsbnAsync(string isbn, bool includeCover = true)
    {
        if (string.IsNullOrWhiteSpace(isbn) || !Regex.IsMatch(isbn, @"^(\d{10}|\d{13})$"))
        {
            throw new ArgumentException("ISBN is not in the correct form.", nameof(isbn));
        }

        var url = _baseUrl + "/volumes?q=isbn:" + isbn;

        var client = new HttpClient()
        {
            Timeout = new TimeSpan(0, 0, 5)
        };
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/117.0");
        //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

        if(throttle && lastRequestTime.HasValue && DateTime.UtcNow - lastRequestTime.Value < TimeSpan.FromMilliseconds(250))
        {
            await Task.Delay(DateTime.UtcNow - lastRequestTime.Value);
        }
        lastRequestTime = DateTime.UtcNow;
        var results = await client.GetFromJsonAsync<VolumeSearchResults>(url);

        if (results == null || results.TotalItems == 0)
        {
            return null;
        }

        var volumeResult = results?.Items?.FirstOrDefault();
        var volumeInfo = volumeResult.VolumeInfo;

        var bookInfo = new BookInformation(
            volumeInfo.Title,
            volumeInfo.Description,
            volumeInfo.Authors,
            volumeInfo.Categories,
            volumeInfo.Publisher,
            volumeInfo.PublishedDate,
            volumeInfo.IndustryIdentifiers?.FirstOrDefault(i => i.Type is "ISBN_13" or "ISBN_10")?.Identifier,
            volumeInfo.PageCount,
            null);

        if(includeCover && !string.IsNullOrWhiteSpace(volumeInfo.ImageLinks?.Thumbnail))
        {
            var cover = await client.GetByteArrayAsync($"https://books.google.com/books/publisher/content/images/frontcover/{volumeResult.Id}?fife=w400-h600");

            bookInfo = bookInfo with
            {
                Cover = cover
            };
        }

        return bookInfo;
    }
}
