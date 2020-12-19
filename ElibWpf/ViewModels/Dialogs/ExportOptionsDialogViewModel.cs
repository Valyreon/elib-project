using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Domain;
using ElibWpf.Models;
using ElibWpf.Models.Options;
using ElibWpf.ValidationAttributes;
using MahApps.Metro.Controls.Dialogs;
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

        public bool IsExportComplete { get; private set; } = false;

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public ICommand ChooseDestinationCommand => new RelayCommand(ChooseDestination);

        [Required(ErrorMessage = "You must specify destination directory.")]
        [DirectoryExists(ErrorMessage = "This directory does not exist.")]
        public string DestinationPath
        {
            get => destinationPath;
            set => Set(() => DestinationPath, ref destinationPath, value);
        }

        public ICommand ExportCommand => new RelayCommand(Export);

        public bool IsGroupByAuthorChecked
        {
            get => groupByAuthor;
            set => Set(() => IsGroupByAuthorChecked, ref groupByAuthor, value);
        }

        public bool IsGroupBySeriesChecked
        {
            get => groupBySeries;
            set => Set(() => IsGroupBySeriesChecked, ref groupBySeries, value);
        }

        private void ChooseDestination()
        {
            using var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                DestinationPath = fbd.SelectedPath;
            }
        }

        private async void Export()
        {
            Validate();
            if (HasErrors)
            {
                return;
            }

            var controlProgress =
                await DialogCoordinator.Instance.ShowProgressAsync(Application.Current.MainWindow.DataContext,
                    "Exporting books", "");
            controlProgress.Minimum = 1;
            controlProgress.Maximum = booksToExport.Count * 2;

            var counter = 0;

            void SetProgress(string message)
            {
                controlProgress.SetMessage("Exporting book: " + message);
                controlProgress.SetProgress(++counter);
            }

            foreach (var b in booksToExport)
            {
                controlProgress.SetMessage("Loading book files...");
                controlProgress.SetProgress(++counter);
            }

            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                var exporter = new Exporter(uow);
                await Task.Run(() => exporter.ExportBooks(booksToExport,
                    new ExporterOptions
                    {
                        DestinationDirectory = DestinationPath,
                        GroupByAuthor = IsGroupByAuthorChecked,
                        GroupBySeries = IsGroupBySeriesChecked
                    }, SetProgress));
            }

            await controlProgress.CloseAsync();
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
        }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
        }
    }
}
