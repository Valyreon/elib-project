using System;
using System.IO;

namespace Models
{
    public static class Logger
    {
        public static void Log(string logCode, string logText)
        {
            File.AppendAllText(ApplicationSettings.GetInstance().LogFilePath,
                $"[TIME: {DateTime.Now:dd\\/MM\\/yyyy h\\:mm tt}](CODE: {logCode}): {logText}\n\n");
        }
    }
}