using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
