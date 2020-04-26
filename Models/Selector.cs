using DataLayer;
using Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class Selector
    {
        private readonly HashSet<int> selectedBookIds;

        public IEnumerable<int> SelectedIds { get => selectedBookIds.AsEnumerable(); }

        public int Count { get => selectedBookIds.Count; }

        public Selector()
        {
            this.selectedBookIds = new HashSet<int>();
        }

        public async Task<IList<Book>> GetSelectedBooks(ElibContext context)
        {
            return await Task.Run(() => context.Books.Where(b => selectedBookIds.Contains(b.Id)).ToList());
        }

        public bool Select(Book book)
        {
            if (book.IsMarked)
            {
                selectedBookIds.Add(book.Id);
                return true;
            }
            else
            {
                selectedBookIds.Remove(book.Id);
                return false;
            }
        }

        public Book SetMarked(Book book)
        {
            book.IsMarked = selectedBookIds.Contains(book.Id);
            return book;
        }
    }
}