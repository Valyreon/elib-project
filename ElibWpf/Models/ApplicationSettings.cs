using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElibWpf.Models
{
    public class ApplicationSettings
    {
        private static ApplicationSettings _instance;

        [JsonPropertyName("DatabasePath")]
        public string
            DatabasePath
        { get; set; } //= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElibApp", "elib_db");

        [JsonPropertyName("LogFilePath")] public string LogFilePath { get; set; } = "log.txt";

        [JsonPropertyName("ForceExportBeforeDelete")] public bool IsExportForcedBeforeDelete = false;

        [JsonIgnore] public string PropertiesPath { get; private set; }

        public static ApplicationSettings GetInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            const string propertiesInCurrentPath = "properties.json";
            var appDataProperties = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ElibApp", "properties.json");

            if (File.Exists("./properties.json"))
            {
                _instance = JsonSerializer.Deserialize<ApplicationSettings>(File.ReadAllText("properties.json"));
                _instance.PropertiesPath = propertiesInCurrentPath;
                return _instance;
            }

            if (File.Exists(appDataProperties))
            {
                _instance = JsonSerializer.Deserialize<ApplicationSettings>(File.ReadAllText(appDataProperties));
                _instance.PropertiesPath = appDataProperties;
                return _instance;
            }

            throw new FileNotFoundException("Couldn't find properties.json in current or in AppData folder.");
        }
    }
}
