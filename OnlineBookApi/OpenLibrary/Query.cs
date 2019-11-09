using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OnlineBookApi.OpenLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookApi.OpenLibrary
{
    public static class Query
    {
        public async static Task<string> GeneralQueryAsync(string queryString)
        {
            queryString = queryString.Trim().Replace(' ', '+');

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://openlibrary.org/search.json?q=" + queryString);

            return await response.Content.ReadAsStringAsync();
        }

        public async static Task<IsbnDetailed> IsbnQueryAsync(string isbn)
        {
            isbn = isbn.Trim();

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://openlibrary.org/api/books?bibkeys=ISBN:" + isbn + "&format=json&jscmd=data");

            JObject jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            JToken jToken = jObject["ISBN:" + isbn];

            return await Task.Run(() => JsonConvert.DeserializeObject<IsbnDetailed>(jToken.ToString()));
        }

        public async static Task<byte[]> GetImageFromOLidAsync(string OLid)
        {

            OLid = OLid.Trim();

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"http://covers.openlibrary.org/b/olid/" + OLid + "-L.jpg");

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
