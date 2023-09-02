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
        private static Selector instance;

        private readonly HashSet<int> selectedBookIds;

        private Selector()
        {
            selectedBookIds = new HashSet<int>();
        }

        public static Selector Instance
        {
            get
            {
                // double checked locking
                if (instance == null)
                {
                    lock (key)
                    {
                        instance ??= new Selector();
                    }
                }

                return instance;
            }
        }

        public int Count => selectedBookIds.Count;

        public IEnumerable<int> SelectedIds => selectedBookIds.AsEnumerable();

        public int LastSelectedId { get; set; }

        public async Task<IList<Book>> GetSelectedBooks(IUnitOfWork uow)
        {
            var result = await uow.BookRepository.FindAsync(Instance.SelectedIds);
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
                return false;
            }

            book.IsMarked = true;
            selectedBookIds.Add(book.Id);

            if (updateLastSelectedId)
            {
                LastSelectedId = book.Id;
            }

            return true;
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
