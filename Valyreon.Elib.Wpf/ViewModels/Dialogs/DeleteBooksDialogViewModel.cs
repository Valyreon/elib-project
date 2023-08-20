using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.Models.Options;
using Valyreon.Elib.Wpf.ValidationAttributes;
using Application = System.Windows.Application;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
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
        }

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
            controlProgress ??=
                await DialogCoordinator.Instance.ShowProgressAsync(Application.Current.MainWindow.DataContext, "", "");

            await Task.Run(async () =>
            {
                var counter = 0;
                controlProgress.Minimum = 1;
                controlProgress.Maximum = booksToExport.Count;
                controlProgress.SetTitle("Deleting books");
                using var uow = await App.UnitOfWorkFactory.CreateAsync();
                foreach (var book in booksToExport)
                {
                    controlProgress.SetMessage("Deleting book: " + book.Title);
                    controlProgress.SetProgress(++counter);
                    await uow.BookRepository.DeleteAsync(book);
                }
                uow.Commit();
            });

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

            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                var exporter = new Exporter(uow);
                exporter.ExportBooks(booksToExport,
                    new ExporterOptions
                    {
                        DestinationDirectory = DestinationPath,
                        GroupByAuthor = IsGroupByAuthorChecked,
                        GroupBySeries = IsGroupBySeriesChecked
                    }, SetProgress);
            }

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
