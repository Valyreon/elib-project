using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Domain;


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

        public IList<Book> GetSelectedBooks(IUnitOfWork uow)
        {
            // TODO: replace this with filter later
            List<Book> result = new List<Book>();
            foreach(int id in selectedBookIds)
            {
                result.Add(uow.BookRepository.Find(id).LoadMembers(uow));
            }
            return result;
        }

        public bool Select(Book book)
        {
            if (book.IsMarked)
            {
                this.selectedBookIds.Add(book.Id);
                return true;
            }

            this.selectedBookIds.Remove(book.Id);
            return false;
        }

        public Book SetMarked(Book book)
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