using DataLayer;
using Domain;
using System.Collections.Generic;
using System.Linq;

namespace ElibWpf.Models
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

		public IList<Book> GetSelectedBooks(IUnitOfWork uow)
		{
			// TODO: replace this with filter later
			var result = new List<Book>();
			foreach(var id in selectedBookIds)
			{
				result.Add(uow.BookRepository.Find(id).LoadMembers(uow));
			}
			return result;
		}

		public bool Select(Book book)
		{
			if(book.IsMarked)
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
