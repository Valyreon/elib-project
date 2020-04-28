﻿using DataLayer;
using Domain;
using Models.Observables;
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

        public bool Select(ObservableBook book)
        {
            if (book.IsSelected)
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

        public ObservableBook SetMarked(ObservableBook book)
        {
            book.IsSelected = selectedBookIds.Contains(book.Id);
            return book;
        }
    }
}