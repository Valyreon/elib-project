using System;
using System.IO;
using DataLayer;
using Models.Options;
using Newtonsoft.Json;

namespace Models
{
    public class ApplicationSettings
    {
        private static ApplicationSettings _instance;

        [JsonProperty("DatabasePath")]
        public string
            DatabasePath { get; set; } //= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElibApp", "elib_db");

        [JsonProperty("LogFilePath")] public string LogFilePath { get; set; } = "log.txt";

        [JsonIgnore] public string PropertiesPath { get; private set; }

        public static ApplicationSettings GetInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            string propertiesInCurrentPath = @"properties.json";
            string appDataProperties = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ElibApp", "properties.json");

            if (File.Exists(@"./properties.json"))
            {
                _instance = JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(@"properties.json"));
                _instance.PropertiesPath = propertiesInCurrentPath;
                return _instance;
            }

            if (File.Exists(appDataProperties))
            {
                _instance = JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(appDataProperties));
                _instance.PropertiesPath = appDataProperties;
                return _instance;
            }

            throw new FileNotFoundException("Couldn't find properties.json in current or in AppData folder.");
        }

        public static IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWork(GetInstance().DatabasePath);
        }
    }
}