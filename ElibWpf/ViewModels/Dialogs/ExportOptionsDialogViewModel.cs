using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using DataLayer;
using Domain;
using ElibWpf.ValidationAttributes;
using MahApps.Metro.Controls.Dialogs;
using Models;
using Models.Options;
using MVVMLibrary;
using Application = System.Windows.Application;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ExportOptionsDialogViewModel : ViewModelWithValidation
    {
        private readonly IList<Book> booksToExport;
        private readonly BaseMetroDialog dialog;

        private string destinationPath;

        private bool groupByAuthor;

        private bool groupBySeries;

        public ExportOptionsDialogViewModel(IList<Book> booksToExport, BaseMetroDialog dialog)
        {
            this.booksToExport = booksToExport;
            this.dialog = dialog;
        }

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public ICommand ChooseDestinationCommand => new RelayCommand(this.ChooseDestination);

        [Required(ErrorMessage = "You must specify destination directory.")]
        [DirectoryExists(ErrorMessage = "This directory does not exist.")]
        public string DestinationPath
        {
            get => this.destinationPath;
            set => this.Set(() => this.DestinationPath, ref this.destinationPath, value);
        }

        public ICommand ExportCommand => new RelayCommand(this.Export);

        public bool IsGroupByAuthorChecked
        {
            get => this.groupByAuthor;
            set => this.Set(() => this.IsGroupByAuthorChecked, ref this.groupByAuthor, value);
        }

        public bool IsGroupBySeriesChecked
        {
            get => this.groupBySeries;
            set => this.Set(() => this.IsGroupBySeriesChecked, ref this.groupBySeries, value);
        }

        private void ChooseDestination()
        {
            using FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                this.DestinationPath = fbd.SelectedPath;
            }
        }

        private async void Export()
        {
            using ElibContext database = ApplicationSettings.CreateContext();
            Exporter exporter = new Exporter(database);

            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, this.dialog);
            ProgressDialogController controlProgress =
                await DialogCoordinator.Instance.ShowProgressAsync(Application.Current.MainWindow.DataContext,
                    "Exporting books", "");
            controlProgress.Minimum = 1;
            controlProgress.Maximum = this.booksToExport.Count * 2;

            int counter = 0;

            void SetProgress(string message)
            {
                controlProgress.SetMessage("Exporting book: " + message);
                controlProgress.SetProgress(++counter);
                Logger.Log("TEST", message);
            }

            foreach (Book b in this.booksToExport)
            {
                controlProgress.SetMessage("Loading book files...");
                controlProgress.SetProgress(++counter);
                database.Books.Attach(b);
                await Task.Run(() => database.Entry(b).Reference(book => book.File).Load());
                await Task.Run(() => database.Entry(b.File).Reference(f => f.RawFile).Load());
            }

            await Task.Run(() => exporter.ExportBooks(this.booksToExport,
                new ExporterOptions
                {
                    DestinationDirectory = this.DestinationPath, GroupByAuthor = this.IsGroupByAuthorChecked,
                    GroupBySeries = this.IsGroupBySeriesChecked
                }, SetProgress));
            await controlProgress.CloseAsync();
        }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, this.dialog);
        }
    }
}