using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.Models
{
    public static class ApplicationData
    {
        private static readonly string _elibDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ELib");

        public static string DatabasePath { get; } = Path.Combine(_elibDataFolder, "elib_db.sqlite");

        public static string LogFolderPath { get; } = Path.Combine(_elibDataFolder, "Logs");

        public static string PropertiesPath { get; } = Path.Combine(_elibDataFolder, "properties.json");

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

        public static void ClearAppData()
        {
            Directory.Delete(_elibDataFolder, true);
        }

        public static ApplicationProperties GetProperties()
        {
            return JsonSerializer.Deserialize<ApplicationProperties>(File.ReadAllText(PropertiesPath));
        }

        public static void SaveProperties(ApplicationProperties properties)
        {
            File.WriteAllText(PropertiesPath, JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public class ApplicationProperties
    {
        public List<SourcePath> Sources { get; set; } = new List<SourcePath>();
        public bool ScanAtStartup { get; set; } = true;
        public List<string> Formats { get; set; } = new List<string> { ".epub", ".mobi" };
    }

    public class SourcePath : ObservableObject
    {
        private string path;
        private bool recursiveScan;

        public string Path
        {
            get => path;
            set => Set(() => Path, ref path, value);
        }

        public bool RecursiveScan
        {
            get => recursiveScan;
            set => Set(() => RecursiveScan, ref recursiveScan, value);
        }
    }
}
