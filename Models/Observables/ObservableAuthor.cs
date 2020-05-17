using Domain;
using GalaSoft.MvvmLight;

namespace Models.Observables
{
    public class ObservableAuthor : ObservableObject
    {
        public ObservableAuthor(Author author)
        {
            this.Author = author;
        }

        public Author Author { get; }

        public int Id => this.Author.Id;

        public string Name
        {
            get => this.Author.Name;
            set
            {
                this.Author.Name = value;
                this.RaisePropertyChanged(() => this.Name);
            }
        }
    }
}