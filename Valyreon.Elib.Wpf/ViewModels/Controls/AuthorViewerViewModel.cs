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
    public class AuthorViewerViewModel : ViewModelBase, IViewer
    {
        private bool isResultEmpty;
        private string caption;

        public ObservableCollection<Author> Authors { get; set; } = new ObservableCollection<Author>();

        public ICommand LoadCommand => new RelayCommand(LoadAllAuthors);
        public ICommand BackCommand => new RelayCommand(Back);

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
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
            MessengerInstance.Register<RefreshCurrentViewMessage>(this, _ => Refresh());
        }

        public void Refresh()
        {
            Authors.Clear();
            LoadAllAuthors();
        }

        public Task<IViewer> Search(SearchParameters searchOptions)
        {
            throw new NotImplementedException();
        }

        public ICommand GoToAuthor => new RelayCommand<Author>(a => Messenger.Default.Send(new AuthorSelectedMessage(a)));

        private async void LoadAllAuthors()
        {
            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            var x = await uow.AuthorRepository.GetAllAsync();

            if (!x.Any())
            {
                IsResultEmpty = true;
                return;
            }

            foreach (var item in x)
            {
                item.NumberOfBooks = await uow.AuthorRepository.CountBooksByAuthorAsync(item.Id);
                Authors.Add(item);
                await Task.Delay(6);
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
