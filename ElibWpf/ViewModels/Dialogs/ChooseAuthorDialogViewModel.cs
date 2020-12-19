using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Domain;
using MahApps.Metro.Controls.Dialogs;
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
            AddedAuthors = addedAuthors;
            this.onConfirm = onConfirm;
        }

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public ICommand DoneCommand => new RelayCommand(Done);

        public ICommand FilterChangedCommand => new RelayCommand(FilterAuthors);

        public string FilterText
        {
            get => filterText;
            set => Set(() => FilterText, ref filterText, value);
        }

        public ICommand LoadAuthorsCommand => new RelayCommand(LoadAuthors);

        public Author SelectedItem
        {
            get => selectedItem;
            set => Set(() => SelectedItem, ref selectedItem, value);
        }

        public ObservableCollection<Author> ShownAuthors { get; set; } = new ObservableCollection<Author>();
        private IEnumerable<int> AddedAuthors { get; }

        private List<Author> AllAuthors { get; } = new List<Author>();

        private void FilterAuthors()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShownAuthors.Clear();
                foreach (var a in AllAuthors.Where(a => a.Name.ToLower().Contains(FilterText.ToLower())))
                {
                    ShownAuthors.Add(a);
                }
            });
        }

        private async void LoadAuthors()
        {
            IEnumerable<Author> list = null;
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                list = uow.AuthorRepository.All();
            }

            foreach (var author in list.Where(a => AddedAuthors.Contains(a.Id)))
            {
                AllAuthors.Add(author);
                ShownAuthors.Add(author);
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
            await Task.Run(() => onConfirm(SelectedItem));
            Cancel();
        }
    }
}
