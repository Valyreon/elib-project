using DataLayer;
using Domain;
using ElibWpf.ValidationAttributes;
using ElibWpf.Views.Dialogs;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Models;
using Models.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ExportOptionsDialogViewModel : ViewModelWithValidation
    {
        private readonly IList<Book> booksToExport;
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly BaseMetroDialog dialog;

        public ExportOptionsDialogViewModel(IList<Book> booksToExport, IDialogCoordinator dialogCoordinator, BaseMetroDialog dialog)
        {
            this.booksToExport = booksToExport;
            this.dialogCoordinator = dialogCoordinator;
            this.dialog = dialog;
        }

        private bool groupByAuthor;
        public bool IsGroupByAuthorChecked
        {
            get => groupByAuthor;
            set => Set(() => IsGroupByAuthorChecked, ref groupByAuthor, value);
        }

        private bool groupBySeries;
        public bool IsGroupBySeriesChecked
        {
            get => groupBySeries;
            set => Set(() => IsGroupBySeriesChecked, ref groupBySeries, value);
        }

        private string destinationPath;
        [Required(ErrorMessage = "You must specify destination directory.")]
        [DirectoryExists(ErrorMessage = "This directory does not exist.")]
        public string DestinationPath
        {
            get => destinationPath;
            set => Set(() => DestinationPath, ref destinationPath, value);
        }

        public ICommand ChooseDestinationCommand { get => new RelayCommand(this.ChooseDestination); }

        private void ChooseDestination()
        {
            using FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                DestinationPath = fbd.SelectedPath;
            }
        }

        public ICommand ExportCommand { get => new RelayCommand(this.Export); }

        private async void Export()
        {
            using ElibContext database = ApplicationSettings.CreateContext();
            Exporter exporter = new Exporter(database);

            await dialogCoordinator.HideMetroDialogAsync(App.Current.MainWindow.DataContext, dialog);
            var controlProgress = await dialogCoordinator.ShowProgressAsync(App.Current.MainWindow.DataContext, "Exporting books", "");
            controlProgress.Minimum = 1;
            controlProgress.Maximum = booksToExport.Count * 2;

            int counter = 0;
            void SetProgress(string message)
            {
                controlProgress.SetMessage("Exporting book: " + message);
                controlProgress.SetProgress(++counter);
                Logger.Log("TEST", message);
            }

            foreach (Book b in booksToExport)
            {
                controlProgress.SetMessage("Loading book files...");
                controlProgress.SetProgress(++counter);
                database.Books.Attach(b);
                await Task.Run(() => database.Entry(b).Reference(b => b.File).Load());
                await Task.Run(() => database.Entry(b.File).Reference(f => f.RawFile).Load());
            }

            await Task.Run(() => exporter.ExportBooks(booksToExport, new ExporterOptions { DestinationDirectory = DestinationPath, GroupByAuthor = IsGroupByAuthorChecked, GroupBySeries = IsGroupBySeriesChecked }, SetProgress));
            await controlProgress.CloseAsync();
        }

        public ICommand CancelCommand { get => new RelayCommand(this.Cancel); }

        private async void Cancel()
        {
            await dialogCoordinator.HideMetroDialogAsync(App.Current.MainWindow.DataContext, dialog);
        }
    }
}
