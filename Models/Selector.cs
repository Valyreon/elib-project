using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Domain;
using Models.Observables;

namespace Models
{
    public class Selector
    {
        private readonly HashSet<int> selectedBookIds;

        public Selector()
        {
            this.selectedBookIds = new HashSet<int>();
        }

        public int Count => this.selectedBookIds.Count;

        public IEnumerable<int> SelectedIds => this.selectedBookIds.AsEnumerable();

        public async Task<IList<Book>> GetSelectedBooks(ElibContext context)
        {
            return await Task.Run(() => context.Books.Where(b => this.selectedBookIds.Contains(b.Id)).ToList());
        }

        public bool Select(ObservableBook book)
        {
            if (book.IsMarked)
            {
                this.selectedBookIds.Add(book.Id);
                return true;
            }

            this.selectedBookIds.Remove(book.Id);
            return false;
        }

        public ObservableBook SetMarked(ObservableBook book)
        {
            book.IsMarked = this.selectedBookIds.Contains(book.Id);
            return book;
        }

        public void Clear()
        {
            this.selectedBookIds.Clear();
        }
    }
}