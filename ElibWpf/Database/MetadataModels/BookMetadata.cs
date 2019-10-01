using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Database.MetadataModels
{
    public class BookMetadata
    {
        public string ISBN { get; set; }
        public string Publisher { get; set; }

        public static BookMetadata GetBookMetadataFromJson(string json)
        {
            return (BookMetadata)JsonConvert.DeserializeObject(json);
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
