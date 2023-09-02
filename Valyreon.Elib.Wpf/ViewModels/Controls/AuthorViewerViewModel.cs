using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.Messages;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
{
    public class AuthorViewerViewModel : ViewModelBase, IViewer, IDisposable
    {
        private bool isResultEmpty;
        private string caption = "Authors";

        public ObservableCollection<Author> Authors { get => authors; set => Set(() => Authors, ref authors, value); }

        public ICommand BackCommand => new RelayCommand(Back);

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        private string searchText;

        public string SearchText
        {
            get => searchText;
            set
            {
                Set(() => SearchText, ref searchText, value);
                Refresh();
            }
        }

        public FilterParameters Filter => null;
        private Action backAction;

        public Action Back
        {
            get => backAction;
            set
            {
                Set(() => Back, ref backAction, value);
                RaisePropertyChanged(() => IsBackEnabled);
            }
        }

        public bool IsBackEnabled => Back != null;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        public AuthorViewerViewModel()
        {
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ =>
            {
                if (!isLoading)
                {
                    Refresh();
                }
            });
        }

        public AuthorViewerViewModel(string searchText) : this()
        {
            this.searchText = searchText;
        }

        public async void Refresh()
        {
            isLoading = true;
            await LoadAuthors();
        }

        private volatile bool isLoading;
        private ObservableCollection<Author> authors;

        public ICommand GoToAuthor => new RelayCommand<Author>(a => Messenger.Default.Send(new AuthorSelectedMessage(a)));

        private async Task LoadAuthors()
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var result = string.IsNullOrWhiteSpace(SearchText)
                ? await uow.AuthorRepository.GetAllAsync()
                : await uow.AuthorRepository.SearchAsync(SearchText);

            if (!result.Any())
            {
                IsResultEmpty = true;
                return;
            }

            foreach (var item in result)
            {
                item.NumberOfBooks = await uow.AuthorRepository.CountBooksByAuthorAsync(item.Id);
            }

            Authors = new ObservableCollection<Author>(result);

            isLoading = false;
        }

        public void Clear()
        {
            Authors.Clear();
        }

        public Func<IViewer> GetCloneFunction()
        {
            var searchToken = searchText;
            return () => new AuthorViewerViewModel(searchToken);
        }

        public void Dispose()
        {
            MessengerInstance.Unregister(this);
        }
    }
}
