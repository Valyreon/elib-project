using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Models
{
    public class Selector
    {
        private readonly HashSet<int> selectedBookIds;

        public Selector()
        {
            selectedBookIds = new HashSet<int>();
        }

        public int Count => selectedBookIds.Count;

        public IEnumerable<int> SelectedIds => selectedBookIds.AsEnumerable();

        public async Task<IList<Book>> GetSelectedBooks(IUnitOfWork uow)
        {
            // TODO: replace this with filter later
            var result = new List<Book>();
            foreach (var id in selectedBookIds)
            {
                var book = await uow.BookRepository.FindAsync(id);
                result.Add(book);
            }
            return result;
        }

        public bool Select(Book book)
        {
            if (book.IsMarked)
            {
                selectedBookIds.Add(book.Id);
                return true;
            }

            selectedBookIds.Remove(book.Id);
            return false;
        }

        public Book SetMarked(Book book)
        {
            book.IsMarked = selectedBookIds.Contains(book.Id);
            return book;
        }

        public void Clear()
        {
            selectedBookIds.Clear();
        }
    }
}
