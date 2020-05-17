using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineBookApi.OpenLibrary.Models;

namespace OnlineBookApi.OpenLibrary
{
    public static class Query
    {
        public enum SearchType
        {
            Query,
            Title,
            Author
        }

        public static async Task<SearchQuery> GeneralQueryAsync(string queryString, SearchType searchType)
        {
            queryString = queryString.Trim().Replace(' ', '+');
            string searchLink = null;

            switch (searchType)
            {
                case SearchType.Title:
                    searchLink = "http://openlibrary.org/search.json?title=";
                    break;

                case SearchType.Author:
                    searchLink = "http://openlibrary.org/search.json?author=";
                    break;

                case SearchType.Query:
                default:
                    searchLink = "http://openlibrary.org/search.json?q=";
                    break;
            }

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(searchLink + queryString);

            string JsonResponse = await response.Content.ReadAsStringAsync();

            return await Task.Run(() => JsonConvert.DeserializeObject<SearchQuery>(JsonResponse));
        }

        public static async Task<IsbnDetailed> IsbnQueryAsync(string isbn)
        {
            isbn = isbn.Trim();

            HttpClient client = new HttpClient();
            HttpResponseMessage response =
                await client.GetAsync("http://openlibrary.org/api/books?bibkeys=ISBN:" + isbn +
                                      "&format=json&jscmd=data");

            JObject jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            JToken jToken = jObject["ISBN:" + isbn];

            return await Task.Run(() => JsonConvert.DeserializeObject<IsbnDetailed>(jToken.ToString()));
        }

        public static async Task<byte[]> GetImageFromOLidAsync(string OLid)
        {
            OLid = OLid.Trim();

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://covers.openlibrary.org/b/olid/" + OLid + "-L.jpg");

            return await response.Content.ReadAsByteArrayAsync();
        }

        public static async Task<byte[]> GetImageFromImageIdAsync(string ImageID)
        {
            ImageID = ImageID.Trim();

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://covers.openlibrary.org/b/id/" + ImageID + "-L.jpg");

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}