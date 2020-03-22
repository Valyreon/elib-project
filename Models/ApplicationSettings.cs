using Models.Options;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Models
{
    public class ApplicationSettings
    {
        private static ApplicationSettings _instance;

        [JsonIgnore]
        public string PropertiesPath { get; private set; }

        public static ApplicationSettings GetInstance()
        {
            if (_instance != null)
                return _instance;

            string propertiesInCurrentPath = @"properties.json";
            string appDataProperties = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElibApp", "properties.json");

            if (File.Exists(@"./properties.json"))
            {
                _instance = JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(@"properties.json"));
                _instance.PropertiesPath = propertiesInCurrentPath;
                return _instance;
            }
            else if (File.Exists(appDataProperties))
            {
                _instance = JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(appDataProperties));
                _instance.PropertiesPath = appDataProperties;
                return _instance;
            }

            throw new FileNotFoundException("Couldn't find properties.json in current or in AppData folder.");
        }

        [JsonProperty("DatabasePath")]
        public string DatabasePath { get; set; } //= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElibApp", "elib_db");

        [JsonProperty("SearchOptions")]
        public SearchOptions SearchOptions { get; set; } = new SearchOptions { SearchByName = true, SearchByAuthor = false, SearchBySeries = false };
    }
}