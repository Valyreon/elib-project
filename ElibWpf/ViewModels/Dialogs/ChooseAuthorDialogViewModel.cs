using DataLayer;
using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Dialogs
{
    public class ChooseAuthorDialogViewModel : ViewModelBase
    {
        private Action<Author> onConfirm;
        private List<Author> allAuthors { get; } = new List<Author>();
        private IEnumerable<int> addedAuthors { get; }


        public ChooseAuthorDialogViewModel(IEnumerable<int> addedAuthors, Action<Author> onConfirm)
        {
            this.addedAuthors = addedAuthors;
            this.onConfirm = onConfirm;
        }

        private Author selectedItem;
        public Author SelectedItem
        {
            get => selectedItem;
            set => Set(() => SelectedItem, ref selectedItem, value);
        }

        private string filterText;
        public string FilterText
        {
            get => filterText;
            set => Set(() => FilterText, ref filterText, value);
        }

        public ICommand FilterChangedCommand { get => new RelayCommand(this.FilterAuthors); }

        private void FilterAuthors()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                //semaphore.Wait();
                ShownAuthors.Clear();
                foreach(Author a in allAuthors.Where(a => a.Name.ToLower().Contains(FilterText.ToLower())))
                {
                    ShownAuthors.Add(a);
                }
                //semaphore.Release();
            });
        }

        public ObservableCollection<Author> ShownAuthors { get; set; } = new ObservableCollection<Author>();

        public ICommand LoadAuthorsCommand { get => new RelayCommand(this.LoadAuthors); }

        private async void LoadAuthors()
        {
            using ElibContext context = ApplicationSettings.CreateContext();
            var list = await context.Authors.Where(a => !addedAuthors.Contains(a.Id)).ToListAsync();
            foreach (var author in list)
            {
                allAuthors.Add(author);
                ShownAuthors.Add(author);
            }
        }

        public ICommand CancelCommand { get => new RelayCommand(this.Cancel); }

        private async void Cancel()
        {
            await DialogCoordinator.Instance.HideMetroDialogAsync(App.Current.MainWindow.DataContext, await DialogCoordinator.Instance.GetCurrentDialogAsync<BaseMetroDialog>(App.Current.MainWindow.DataContext));
        }

        public ICommand DoneCommand { get => new RelayCommand(this.Done); }

        private async void Done()
        {
            await Task.Run(() => onConfirm(SelectedItem));
            Cancel();
        }
    }
}
