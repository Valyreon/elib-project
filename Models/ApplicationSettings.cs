using Newtonsoft.Json;
using System.IO;

namespace Models
{
    public class ApplicationSettings
    {
        private static ApplicationSettings _instance;

        public static ApplicationSettings GetInstance()
        {
            if (_instance == null)
            {
                if (!File.Exists(@"./properties.json"))
                    throw new FileNotFoundException("Couldn't find properties.json file. Read instructions in README.md and make one.");
                _instance = JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(@"properties.json"));
            }
            return _instance;
        }

        [JsonProperty("DatabasePath")]
        public string DatabasePath { get; set; }
    }
}
