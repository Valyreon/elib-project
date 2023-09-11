using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public record ApplicationProperties
    {
        public List<SourcePath> Sources { get; set; } = new List<SourcePath>();
        public bool ScanAtStartup { get; set; } = true;
        public string ExternalReaderPath { get; set; }
        public bool RememberFilterInNextView { get; set; } = false;
        public bool AutomaticallyImportWithFoundISBN { get; set; } = true;
        public List<string> Formats { get; set; } = new List<string> { ".epub", ".mobi" };

        public bool IsExternalReaderSpecifiedAndValid()
        {
            return !string.IsNullOrWhiteSpace(ExternalReaderPath) && File.Exists(ExternalReaderPath);
        }

        // override copy constructor for record
        protected ApplicationProperties(ApplicationProperties other)
        {
            ScanAtStartup = other.ScanAtStartup;
            ExternalReaderPath = other.ExternalReaderPath;
            Formats = other.Formats.ToList();
            Sources = other.Sources.Select(s => s with { }).ToList();
        }
    }

    public record SourcePath
    {
        public string Path { get; set; }
        public bool RecursiveScan { get; set; }
    }
}
