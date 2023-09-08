using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class ApplicationSettingsViewModel : ViewModelBase, ITabViewModel
    {
        private bool scanAtStartup;

        public bool ScanAtStartup
        {
            get => scanAtStartup;
            set => Set(() => ScanAtStartup, ref scanAtStartup, value);
        }

        public bool AutomaticallyImportWithFoundISBN
        {
            get => automaticallyImportWithFoundISBN;
            set => Set(() => AutomaticallyImportWithFoundISBN, ref automaticallyImportWithFoundISBN, value);
        }

        public string SelectedFormat { get => selectedFormat; set => Set(() => SelectedFormat, ref selectedFormat, value); }

        private string externalReaderPath;
        private string caption = "Settings";
        private bool automaticallyImportWithFoundISBN;
        private string selectedFormat;
        private readonly ApplicationProperties properties;

        public string ExternalReaderPath
        {
            get => externalReaderPath;
            set => Set(() => ExternalReaderPath, ref externalReaderPath, value);
        }

        public ObservableCollection<SourcePathObservable> SourcePaths { get; set; }

        public ObservableCollection<string> Formats { get; set; }

        public SourcePathObservable SelectedItem { get; set; }

        public ApplicationSettingsViewModel(ApplicationProperties properties)
        {
            SourcePaths = new ObservableCollection<SourcePathObservable>(properties.Sources.Select(s => new SourcePathObservable(s)));
            ScanAtStartup = properties.ScanAtStartup;
            Formats = new(properties.Formats);
            ExternalReaderPath = properties.ExternalReaderPath;
            this.properties = properties;

            // TODO Stop subscribing with anonymous methods here since we cant unsubscribe
            SourcePaths.CollectionChanged += (_, _) => SaveChanges();
            Formats.CollectionChanged += (_, _) => SaveChanges();
            PropertyChanged += (_, _) => SaveChanges();
            foreach (var item in SourcePaths)
            {
                item.PropertyChanged += (_, _) => SaveChanges();
            }
        }

        public ICommand AddSourceCommand => new RelayCommand(HandleAddSource);
        public ICommand RemoveSourceCommand => new RelayCommand(HandleRemoveSource);

        private void HandleRemoveSource()
        {
            SourcePaths.Remove(SelectedItem);
        }

        public ICommand ChooseExternalReaderCommand => new RelayCommand(HandleChooseExternalReader);

        public ICommand AddFormatCommand => new RelayCommand(HandleAddFormat);
        public ICommand RemoveFormatCommand => new RelayCommand(() => Formats.Remove(SelectedFormat));

        public ICommand ClearExternalReaderCommand => new RelayCommand(() => ExternalReaderPath = null);

        private void HandleAddFormat()
        {
            var dialogViewModel = new SimpleTextInputDialogViewModel("Add Format", "Input extensions you want the app to scan for. For example '.epub' or 'pdf'.", str =>
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return;
                }

                str = str.ToLowerInvariant().Trim();

                if (str.StartsWith(".") == false)
                {
                    str = "." + str;
                }

                if (!Regex.IsMatch(str, @"\.[a-zA-Z0-9]+"))
                {
                    MessengerInstance.Send(new ShowNotificationMessage("Invalid file format.", NotificationType.Error));
                    return;
                }

                if (Formats.Contains(str))
                {
                    return;
                }

                Formats.Add(str);
            });

            MessengerInstance.Send(new ShowDialogMessage(dialogViewModel));
        }

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        private void HandleChooseExternalReader()
        {
            using var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 0,
                Multiselect = false,
                InitialDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%")
            };

            var result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileName != null)
            {
                ExternalReaderPath = dlg.FileName;
            }
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

            if (dialog.ShowDialog() == DialogResult.OK && !SourcePaths.Any(s => s.Path == dialog.SelectedPath))
            {
                var obs = new SourcePathObservable(new SourcePath
                {
                    Path = dialog.SelectedPath,
                    RecursiveScan = false
                });

                obs.PropertyChanged += (_, _) => SaveChanges();

                SourcePaths.Add(obs);
            }
        }

        private void UpdateCurrentSettings()
        {
            properties.ScanAtStartup = ScanAtStartup;
            properties.Sources = SourcePaths.Select(s => s.GetSourcePath()).ToList();
            properties.Formats = Formats.ToList();
            properties.AutomaticallyImportWithFoundISBN = AutomaticallyImportWithFoundISBN;
            properties.ExternalReaderPath = ExternalReaderPath;
        }

        private void SaveChanges()
        {
            UpdateCurrentSettings();
            ApplicationData.SaveProperties(properties);
            MessengerInstance.Send(new AppSettingsChangedMessage());
        }
    }
}
