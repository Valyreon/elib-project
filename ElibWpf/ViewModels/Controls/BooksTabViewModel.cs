using Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Controls
{
    public class BooksTabViewModel : ViewModelBase, IPageViewModel
    {
        private string caption = "Books";
        public string Caption
        {
            get => caption;
            set => Set(ref caption, value);
        }

        public List<UserCollection> Collections { get; set; } = App.Database.UserCollections.ToList();

        private UserCollection selectedCollection;
        public UserCollection SelectedCollection
        {
            get => selectedCollection;

            set 
            {
                Set(ref selectedCollection, value);
                if(selectedCollection != null)
                {
                    CurrentViewer = new BookViewerViewModel($"Collection {selectedCollection.Tag}",(Book x) => x.UserCollections.Where(c => c.Id == SelectedCollection.Id).Count() > 0);
                }
            }
        }

        private ISearchable currentViewer;
        public ISearchable CurrentViewer
        {
            get => currentViewer; 
            set => Set(ref currentViewer, value);
        }

        public ICommand ViewCollectionCommand { get => new RelayCommand(this.ViewCollection); }

        private void ViewCollection()
        {
            if(SelectedCollection != null)
            {
                
            }
        }
    }
}
