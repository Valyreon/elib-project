using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Valyreon.Elib.Wpf.Models
{
    public static class ApplicationData
    {
        private static readonly string _elibDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ELib");

        public static string DatabasePath { get; } = Path.Combine(_elibDataFolder, "elib_db.sqlite");

        public static string LogFolderPath { get; } = Path.Combine(_elibDataFolder, "Logs");

        public static string PropertiesPath { get; } = Path.Combine(_elibDataFolder, "properties.json");

        public static void ClearAppData()
        {
            Directory.Delete(_elibDataFolder, true);
        }

        public static ApplicationProperties GetProperties()
        {
            try
            {
                return JsonSerializer.Deserialize<ApplicationProperties>(File.ReadAllText(PropertiesPath));
            }
            catch (Exception)
            {
                return new ApplicationProperties();
            }
        }

        public static void InitializeAppData()
        {
            if (!Directory.Exists(_elibDataFolder))
            {
                Directory.CreateDirectory(_elibDataFolder);
            }

            if (!File.Exists(DatabasePath))
            {
                File.Copy("Resources/empty_database.sqlite", DatabasePath);
            }

            if (!Directory.Exists(LogFolderPath))
            {
                Directory.CreateDirectory(LogFolderPath);
            }

            if (!File.Exists(PropertiesPath))
            {
                File.WriteAllText(PropertiesPath, JsonSerializer.Serialize(new ApplicationProperties(), new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        public static void SaveProperties(ApplicationProperties properties)
        {
            File.WriteAllText(PropertiesPath, JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public class ApplicationProperties
    {
        public bool AutomaticallyImportWithFoundISBN { get; set; }
        public bool AutomaticallyImportBooksWithValidData { get; set; }
        public List<string> Formats { get; set; } = new List<string> { ".epub", ".mobi" };
        public string LibraryFolder { get; set; }
        public bool RememberFilterInNextView { get; set; }
        public bool ScanAtStartup { get; set; } = true;
    }
}
