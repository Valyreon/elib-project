using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Domain;
using ElibWpf.Interfaces;
using ElibWpf.Models;
using ElibWpf.Models.Options;
using ElibWpf.ValidationAttributes;
using MahApps.Metro.Controls.Dialogs;
using MVVMLibrary;
using Application = System.Windows.Application;

namespace ElibWpf.ViewModels.Dialogs
{
    public class DeleteBooksDialogViewModel : ViewModelWithValidation, IActionOnClose
    {
        private readonly IList<Book> booksToExport;
        private readonly BaseMetroDialog dialog;

        private string destinationPath;

        private bool groupByAuthor;

        private bool groupBySeries;
        private Action onCloseAction;

        public DeleteBooksDialogViewModel(IList<Book> booksToExport, BaseMetroDialog dialog)
        {
            this.booksToExport = booksToExport;
            this.dialog = dialog;
            IsExportPartVisible = ApplicationSettings.GetInstance().IsExportForcedBeforeDelete;
        }

        public bool IsExportPartVisible { get; set; }

        public ICommand CancelCommand => new RelayCommand(HandleCancel);

        private async void HandleCancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
        }

        public ICommand ChooseDestinationCommand => new RelayCommand(ChooseDestination);

        [Required(ErrorMessage = "You must specify destination directory.")]
        [DirectoryExists(ErrorMessage = "This directory does not exist.")]
        public string DestinationPath
        {
            get => destinationPath;
            set => Set(() => DestinationPath, ref destinationPath, value);
        }

        public ICommand ExportCommand => new RelayCommand(Export);

        public ICommand ContinueCommand => new RelayCommand(ContinueDeletionProxy);

        private async void ContinueDeletionProxy()
        {
            var cp = await ContinueDeletion();
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
            await cp.CloseAsync();
            onCloseAction();
        }

        private async Task<ProgressDialogController> ContinueDeletion(ProgressDialogController controlProgress = null)
        {
            if (controlProgress == null)
            {
                controlProgress =
                await DialogCoordinator.Instance.ShowProgressAsync(Application.Current.MainWindow.DataContext, "", "");
            }

            using var uow = ApplicationSettings.CreateUnitOfWork();
            await Task.Run(async () =>
            {
                var counter = 0;
                controlProgress.Minimum = 1;
                controlProgress.Maximum = booksToExport.Count;
                controlProgress.SetTitle("Deleting books");
                foreach (var book in booksToExport)
                {
                    controlProgress.SetMessage("Deleting book: " + book.Title);
                    controlProgress.SetProgress(++counter);
                    uow.BookRepository.Remove(book);
                    await Task.Delay(50);
                }
                uow.ClearCache();
                uow.Commit();
            });

            uow.Dispose();
            return controlProgress;
        }

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
            using var uow = ApplicationSettings.CreateUnitOfWork();
            var exporter = new Exporter(uow);

            var controlProgress =
                await DialogCoordinator.Instance.ShowProgressAsync(Application.Current.MainWindow.DataContext,
                    "Exporting books", "");
            controlProgress.Minimum = 1;
            controlProgress.Maximum = booksToExport.Count * 2;

            var counter = 0;

            async void SetProgress(string message)
            {
                controlProgress.SetMessage("Exporting book: " + message);
                controlProgress.SetProgress(++counter);
                await Task.Delay(50);
            }

            foreach (var b in booksToExport)
            {
                controlProgress.SetMessage("Loading book files...");
                controlProgress.SetProgress(++counter);
                await Task.Delay(50);
            }

            await Task.Run(() => exporter.ExportBooks(booksToExport,
                new ExporterOptions
                {
                    DestinationDirectory = DestinationPath,
                    GroupByAuthor = IsGroupByAuthorChecked,
                    GroupBySeries = IsGroupBySeriesChecked
                }, SetProgress));
            uow.Dispose();

            await ContinueDeletion(controlProgress);

            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
            await controlProgress.CloseAsync();
            onCloseAction();
        }

        public void SetActionOnClose(Action action)
        {
            onCloseAction = action;
        }

        public async void Close()
        {
            onCloseAction();
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext, dialog);
        }
    }
}
