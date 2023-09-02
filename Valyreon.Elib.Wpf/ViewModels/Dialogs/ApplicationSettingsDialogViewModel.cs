using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class ApplicationSettingsDialogViewModel : DialogViewModel
    {
        private bool scanAtStartup;

        public bool ScanAtStartup
        {
            get => scanAtStartup;
            set => Set(() => ScanAtStartup, ref scanAtStartup, value);
        }

        private bool scanEpub;

        public bool ScanEpub
        {
            get => scanEpub;
            set => Set(() => ScanEpub, ref scanEpub, value);
        }

        private bool scanMobi;

        public bool ScanMobi
        {
            get => scanMobi;
            set => Set(() => ScanMobi, ref scanMobi, value);
        }

        private bool scanPdf;

        public bool ScanPdf
        {
            get => scanPdf;
            set => Set(() => ScanPdf, ref scanPdf, value);
        }

        private bool scanAzw;

        public bool ScanAzw
        {
            get => scanAzw;
            set => Set(() => ScanAzw, ref scanAzw, value);
        }

        public ObservableCollection<SourcePath> SourcePaths { get; set; }

        public SourcePath SelectedItem { get; set; }

        public ApplicationSettingsDialogViewModel()
        {
            var properties = ApplicationData.GetProperties();
            SourcePaths = new ObservableCollection<SourcePath>(properties.Sources);
            ScanAtStartup = properties.ScanAtStartup;

            if (properties.Formats.Any(f => f.ToLowerInvariant() == ".epub"))
            {
                ScanEpub = true;
            }

            if (properties.Formats.Any(f => f.ToLowerInvariant() == ".mobi"))
            {
                ScanMobi = true;
            }

            if (properties.Formats.Any(f => f.ToLowerInvariant() == ".pdf"))
            {
                ScanPdf = true;
            }

            if (properties.Formats.Any(f => f.ToLowerInvariant() == ".azw"))
            {
                ScanAzw = true;
            }
        }

        public ICommand AddSourceCommand => new RelayCommand(HandleAddSource);
        public ICommand RemoveSourceCommand => new RelayCommand(HandleRemoveSource);
        public ICommand SaveCommand => new RelayCommand(Save);
        public ICommand CancelCommand => new RelayCommand(Close);

        private void HandleScan()
        {
            throw new NotImplementedException();
        }

        private void HandleRemoveSource()
        {
            SourcePaths.Remove(SelectedItem);
        }

        private void HandleAddSource()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select source folder",
                UseDescriptionForTitle = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    + Path.DirectorySeparatorChar,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourcePaths.Add(new SourcePath
                {
                    Path = dialog.SelectedPath,
                    RecursiveScan = false
                });
            }
        }

        private void Save()
        {
            var newProperties = new ApplicationProperties
            {
                ScanAtStartup = ScanAtStartup,
                Sources = SourcePaths.ToList(),
                Formats = new()
            };

            if (ScanEpub)
            {
                newProperties.Formats.Add(".epub");
            }

            if (ScanMobi)
            {
                newProperties.Formats.Add(".mobi");
            }

            if (ScanAzw)
            {
                newProperties.Formats.Add(".azw");
            }

            if (ScanPdf)
            {
                newProperties.Formats.Add(".pdf");
            }

            ApplicationData.SaveProperties(newProperties);
            Close();

            MessengerInstance.Send(new ScanForNewBooksMessage());
            MessengerInstance.Send(new AppSettingsChangedMessage());
        }
    }
}
