using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Interfaces;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.Models.Options;
using Valyreon.Elib.Wpf.ValidationAttributes;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class ExportOptionsDialogViewModel : DialogViewModel
    {
        private readonly IList<Book> booksToExport;
        private readonly IUnitOfWorkFactory uowFactory;
        private string destinationPath;

        private bool groupByAuthor;

        private bool groupBySeries;

        public ExportOptionsDialogViewModel(IList<Book> booksToExport, IUnitOfWorkFactory uowFactory)
        {
            this.booksToExport = booksToExport;
            this.uowFactory = uowFactory;
        }

        public ICommand CancelCommand => new RelayCommand(Close);
        public ICommand ChooseDestinationCommand => new RelayCommand(ChooseDestination);

        [Required(ErrorMessage = "You must specify destination directory.")]
        [DirectoryExists(ErrorMessage = "This directory does not exist.")]
        public string DestinationPath
        {
            get => destinationPath;
            set => Set(() => DestinationPath, ref destinationPath, value);
        }

        public ICommand ExportCommand => new RelayCommand(Export);
        public bool IsExportComplete { get; }

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

            var progressDialog = new ProgressBarWithMessageDialogViewModel
            {
                BarMaximum = booksToExport.Count * 2,
                BarMinimum = 1,
                Title = "Exporting books"
            };
            Close();
            MessengerInstance.Send(new ShowDialogMessage(progressDialog));

            var counter = 0;

            void SetProgress(string message)
            {
                progressDialog.CurrentMessage = message;
                progressDialog.CurrentBarValue = ++counter;
            }

            using (var uow = await uowFactory.CreateAsync())
            {
                foreach (var b in booksToExport)
                {
                    progressDialog.CurrentMessage = "Loading books...";
                    if (!booksToExport[counter].IsLoaded)
                    {
                        await booksToExport[counter].LoadBookAsync(uow);
                    }
                    progressDialog.CurrentBarValue = ++counter;
                }
            }

            using (var uow = await uowFactory.CreateAsync())
            {
                await Task.Run(() =>
                {
                    var exporter = new Exporter();
                    exporter.ExportBooks(booksToExport,
                        new ExporterOptions
                        {
                            DestinationDirectory = DestinationPath,
                            GroupByAuthor = IsGroupByAuthorChecked,
                            GroupBySeries = IsGroupBySeriesChecked
                        }, SetProgress);
                    MessengerInstance.Send(new ShowNotificationMessage($"{booksToExport.Count} books exported."));
                });
            }

            Close();
        }
    }
}
