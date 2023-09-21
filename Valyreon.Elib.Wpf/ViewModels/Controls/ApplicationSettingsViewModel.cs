using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.Services;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.ViewModels.Flyouts;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class ApplicationSettingsViewModel : ViewModelBase, ITabViewModel
    {
        private static readonly Regex _regex = new Regex(@"\.[a-zA-Z0-9]+");
        private readonly ApplicationProperties properties;
        private readonly IUnitOfWorkFactory uowFactory;
        private bool automaticallyImportWithFoundISBN;
        private string caption = "Settings";
        private string externalReaderPath;
        private string libraryPath;
        private bool scanAtStartup;

        private string selectedFormat;

        public ApplicationSettingsViewModel(ApplicationProperties properties, IUnitOfWorkFactory uowFactory)
        {
            LibraryPath = properties.LibraryFolder;
            ScanAtStartup = properties.ScanAtStartup;
            Formats = new(properties.Formats);
            ExternalReaderPath = properties.ExternalReaderPath;
            AutomaticallyImportWithFoundISBN = properties.AutomaticallyImportWithFoundISBN;
            this.properties = properties;
            this.uowFactory = uowFactory;

            // TODO Stop subscribing with anonymous methods here since we cant unsubscribe
            Formats.CollectionChanged += (_, _) => SaveChanges();
            PropertyChanged += (_, _) => SaveChanges();
        }

        public ICommand AddFormatCommand => new RelayCommand(HandleAddFormat);

        public bool AutomaticallyImportWithFoundISBN
        {
            get => automaticallyImportWithFoundISBN;
            set => Set(() => AutomaticallyImportWithFoundISBN, ref automaticallyImportWithFoundISBN, value);
        }

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        public ICommand ChooseExternalReaderCommand => new RelayCommand(HandleChooseExternalReader);
        public ICommand ChooseLibraryCommand => new RelayCommand(HandleChooseLibrary);

        public ICommand ClearExternalReaderCommand => new RelayCommand(() => ExternalReaderPath = null);

        public string ExternalReaderPath
        {
            get => externalReaderPath;
            set => Set(() => ExternalReaderPath, ref externalReaderPath, value);
        }

        public ObservableCollection<string> Formats { get; set; }
        public string LibraryPath { get => libraryPath; set => Set(() => LibraryPath, ref libraryPath, value); }
        public ICommand RemoveFormatCommand => new RelayCommand(() => Formats.Remove(SelectedFormat));

        public bool ScanAtStartup
        {
            get => scanAtStartup;
            set => Set(() => ScanAtStartup, ref scanAtStartup, value);
        }

        public ICommand ScanLibraryCommand => new RelayCommand(HandleScanLibraryForNewContent);

        public string SelectedFormat { get => selectedFormat; set => Set(() => SelectedFormat, ref selectedFormat, value); }

        private void HandleAddFormat()
        {
            var dialogViewModel = new SimpleTextInputDialogViewModel("Add Format", "Input extensions you want the app to scan for. For example '.epub' or 'pdf'.", str =>
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return;
                }

                str = str.ToLowerInvariant().Trim();

                if (!str.StartsWith("."))
                {
                    str = "." + str;
                }

                if (!_regex.IsMatch(str))
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

        private void HandleChooseExternalReader()
        {
            using var dlg = new System.Windows.Forms.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 0,
                Multiselect = false,
                InitialDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%")
            };

            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && dlg.FileName != null)
            {
                ExternalReaderPath = dlg.FileName;
            }
        }

        private void HandleChooseLibrary()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select library folder",
                UseDescriptionForTitle = true,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    + Path.DirectorySeparatorChar,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LibraryPath = dialog.SelectedPath;

                // TODO: what to do with already imported files if user changes library?
            }
        }

        private async void HandleScanLibraryForNewContent()
        {
            var importer = new ImportService(uowFactory, properties);

            MessengerInstance.Send(new SetGlobalLoaderMessage(true));
            var newBookPaths = await importer.GetNotImportedBookPathsAsync(properties.LibraryFolder);
            MessengerInstance.Send(new SetGlobalLoaderMessage(false));
            if (!newBookPaths.Any())
            {
                MessengerInstance.Send(new ShowNotificationMessage("No new books found in the library."));
                return;
            }

            var importFlyout = new AddNewBooksViewModel(newBookPaths, uowFactory);

            Application.Current.Dispatcher.Invoke(() => MessengerInstance.Send(new OpenFlyoutMessage(importFlyout)));
        }

        private void SaveChanges()
        {
            UpdateCurrentSettings();
            ApplicationData.SaveProperties(properties);
            MessengerInstance.Send(new AppSettingsChangedMessage());
        }

        private void UpdateCurrentSettings()
        {
            properties.ScanAtStartup = ScanAtStartup;
            properties.LibraryFolder = LibraryPath;
            properties.Formats = Formats.ToList();
            properties.AutomaticallyImportWithFoundISBN = AutomaticallyImportWithFoundISBN;
            properties.ExternalReaderPath = ExternalReaderPath;
        }
    }
}
