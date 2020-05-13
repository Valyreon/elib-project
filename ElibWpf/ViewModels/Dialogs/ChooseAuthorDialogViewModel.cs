using DataLayer;
using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Models;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ChooseAuthorDialogViewModel : ViewModelBase
    {
        private Action<Author> onConfirm;
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly BaseMetroDialog dialog;

        public ChooseAuthorDialogViewModel(Action<Author> onConfirm, IDialogCoordinator dialogCoordinator, BaseMetroDialog dialog)
        {
            this.onConfirm = onConfirm;
            this.dialogCoordinator = dialogCoordinator;
            this.dialog = dialog;
        }

        public ObservableCollection<Author> Authors { get; set; } = new ObservableCollection<Author>();

        public ICommand LoadAuthorsCommand { get => new RelayCommand(this.LoadAuthors); }

        private async void LoadAuthors()
        {
            using ElibContext context = ApplicationSettings.CreateContext();
            var list = await context.Authors.ToListAsync();
            foreach (var author in list)
                Authors.Add(author);
        }

        public ICommand CancelCommand { get => new RelayCommand(this.Cancel); }

        private async void Cancel()
        {
            await dialogCoordinator.HideMetroDialogAsync(App.Current.MainWindow.DataContext,dialog);
        }
    }
}
