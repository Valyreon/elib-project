using Newtonsoft.Json;

namespace Models.Options
{
    public class SearchOptions
    {
        [JsonProperty("SearchByName")]
        public bool SearchByName { get; set; } = true;

        [JsonProperty("SearchBySeries")]
        public bool SearchBySeries { get; set; } = false;

        [JsonProperty("SearchByAuthor")]
        public bool SearchByAuthor { get; set; } = false;
    }
}