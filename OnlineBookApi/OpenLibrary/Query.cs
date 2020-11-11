using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineBookApi.OpenLibrary.Models;
using System.Net.Http;
using System.Threading.Tasks;

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

			searchLink = searchType switch
			{
				SearchType.Title => "http://openlibrary.org/search.json?title=",
				SearchType.Author => "http://openlibrary.org/search.json?author=",
				_ => "http://openlibrary.org/search.json?q=",
			};
			var client = new HttpClient();
			var response = await client.GetAsync(searchLink + queryString);

			var JsonResponse = await response.Content.ReadAsStringAsync();

			return await Task.Run(() => JsonConvert.DeserializeObject<SearchQuery>(JsonResponse));
		}

		public static async Task<IsbnDetailed> IsbnQueryAsync(string isbn)
		{
			isbn = isbn.Trim();

			var client = new HttpClient();
			var response =
				await client.GetAsync("http://openlibrary.org/api/books?bibkeys=ISBN:" + isbn +
									  "&format=json&jscmd=data");

			var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
			var jToken = jObject["ISBN:" + isbn];

			return await Task.Run(() => JsonConvert.DeserializeObject<IsbnDetailed>(jToken.ToString()));
		}

		public static async Task<byte[]> GetImageFromOLidAsync(string OLid)
		{
			OLid = OLid.Trim();

			var client = new HttpClient();
			var response = await client.GetAsync("http://covers.openlibrary.org/b/olid/" + OLid + "-L.jpg");

			return await response.Content.ReadAsByteArrayAsync();
		}

		public static async Task<byte[]> GetImageFromImageIdAsync(string ImageID)
		{
			ImageID = ImageID.Trim();

			var client = new HttpClient();
			var response = await client.GetAsync("http://covers.openlibrary.org/b/id/" + ImageID + "-L.jpg");

			return await response.Content.ReadAsByteArrayAsync();
		}
	}
}
