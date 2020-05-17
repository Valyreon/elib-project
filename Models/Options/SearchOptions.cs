using Newtonsoft.Json;

namespace Models.Options
{
    public class SearchOptions
    {
        [JsonProperty("SearchByAuthor")] public bool SearchByAuthor { get; set; }
        [JsonProperty("SearchByName")] public bool SearchByName { get; set; } = true;

        [JsonProperty("SearchBySeries")] public bool SearchBySeries { get; set; }
    }
}