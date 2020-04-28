using Domain;
using GalaSoft.MvvmLight;

namespace Models.Observables
{
    public class ObservableAuthor: ObservableObject
    {
        private readonly Author author;

        public Author Author { get => author; }

        public ObservableAuthor(Author author)
        {
            this.author = author;
        }

        public string Name
        {
            get => author.Name;
            set
            {
                this.author.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public int Id { get => author.Id; }
    }
}
