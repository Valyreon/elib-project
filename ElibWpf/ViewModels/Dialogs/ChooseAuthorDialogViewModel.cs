using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DataLayer;
using Domain;
using MahApps.Metro.Controls.Dialogs;
using Models;
using MVVMLibrary;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ChooseAuthorDialogViewModel : ViewModelBase
    {
        private readonly Action<Author> onConfirm;
        private string filterText;
        private Author selectedItem;

        public ChooseAuthorDialogViewModel(IEnumerable<int> addedAuthors, Action<Author> onConfirm)
        {
            this.AddedAuthors = addedAuthors;
            this.onConfirm = onConfirm;
        }

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public ICommand DoneCommand => new RelayCommand(this.Done);

        public ICommand FilterChangedCommand => new RelayCommand(this.FilterAuthors);

        public string FilterText
        {
            get => this.filterText;
            set => this.Set(() => this.FilterText, ref this.filterText, value);
        }

        public ICommand LoadAuthorsCommand => new RelayCommand(this.LoadAuthors);

        public Author SelectedItem
        {
            get => this.selectedItem;
            set => this.Set(() => this.SelectedItem, ref this.selectedItem, value);
        }

        public ObservableCollection<Author> ShownAuthors { get; set; } = new ObservableCollection<Author>();
        private IEnumerable<int> AddedAuthors { get; }

        private List<Author> AllAuthors { get; } = new List<Author>();

        private void FilterAuthors()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ShownAuthors.Clear();
                foreach (Author a in this.AllAuthors.Where(a => a.Name.ToLower().Contains(this.FilterText.ToLower())))
                {
                    this.ShownAuthors.Add(a);
                }
            });
        }

        private void LoadAuthors()
        {
            using var uow = ApplicationSettings.CreateUnitOfWork();
            var list = uow.AuthorRepository.All().ToList();
            list.RemoveAll(a => this.AddedAuthors.Contains(a.Id));
            foreach (Author author in list)
            {
                this.AllAuthors.Add(author);
                this.ShownAuthors.Add(author);
            }
        }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(Application.Current.MainWindow.DataContext,
                await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(Application.Current.MainWindow
                    .DataContext));
        }

        private async void Done()
        {
            await Task.Run(() => this.onConfirm(this.SelectedItem));
            this.Cancel();
        }
    }
}