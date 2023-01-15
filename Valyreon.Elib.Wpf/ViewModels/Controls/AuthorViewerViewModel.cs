using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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

        public ICommand LoadCommand => new RelayCommand(LoadMore);
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

        public void Clear()
        {
            Authors.Clear();
        }

        public void Refresh()
        {
            Authors.Clear();
            UnitOfWork.ClearCache();
            LoadMore();
        }

        public Task<IViewer> Search(SearchParameters searchOptions)
        {
            throw new NotImplementedException();
        }

        public ICommand GoToAuthor => new RelayCommand<Author>(a => Messenger.Default.Send(new AuthorSelectedMessage(a)));

        private async void LoadMore()
        {
            await Task.Factory.StartNew(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();
                return uow.AuthorRepository.All().ToList();
            }).ContinueWith((x) =>
            {
                if (x.Result.Count == 0)
                {
                    IsResultEmpty = true;
                    return;
                }

                foreach (var item in x.Result)
                {
                    Authors.Add(item);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
