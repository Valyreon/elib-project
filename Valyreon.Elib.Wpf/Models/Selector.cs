using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Models
{
    public class Selector
    {
        private static readonly object key = new object();

        private readonly HashSet<int> selectedBookIds;

        public Selector()
        {
            selectedBookIds = new HashSet<int>();
        }

        public event Action SelectionChanged;

        public int Count => selectedBookIds.Count;

        public int LastSelectedId { get; set; }
        public IEnumerable<int> SelectedIds => selectedBookIds.AsEnumerable();

        public void Clear()
        {
            selectedBookIds.Clear();
            SelectionChanged.Invoke();
        }

        public void DeselectIds(IEnumerable<int> ids)
        {
            var changed = false;
            foreach (var id in ids)
            {
                if (selectedBookIds.Remove(id))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                SelectionChanged.Invoke();
            }
        }

        public async Task<IList<Book>> GetSelectedBooks(IUnitOfWork uow)
        {
            var result = await uow.BookRepository.FindAsync(SelectedIds);
            return result.ToList();
        }

        public bool Select(Book book, bool updateLastSelectedId = true)
        {
            if (book.Id == 0)
            {
                throw new ArgumentException("Can't select book which are not added to DB.");
            }

            if (selectedBookIds.Contains(book.Id))
            {
                book.IsMarked = false;
                selectedBookIds.Remove(book.Id);
                SelectionChanged.Invoke();
                return false;
            }

            book.IsMarked = true;
            selectedBookIds.Add(book.Id);

            if (updateLastSelectedId)
            {
                LastSelectedId = book.Id;
            }

            SelectionChanged.Invoke();

            return true;
        }

        public void SelectIds(IEnumerable<int> ids)
        {
            var changed = false;
            foreach (var id in ids)
            {
                if (selectedBookIds.Add(id))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                SelectionChanged.Invoke();
            }
        }

        public Book SetMarked(Book book)
        {
            book.IsMarked = selectedBookIds.Contains(book.Id);
            return book;
        }
    }
}
