using Domain;
using Models;

namespace ElibWpf.BindingItems
{
    public class QuoteJsHelper
    {
        private Book book;

        public QuoteJsHelper(Book book)
        {
            this.book = book;
        }

        public void RegisterQuote(string quote, string note = null)
        {
            Quote newQuote = new Quote
            {
                Text = quote,
                Note = note,
                BookId = this.book.Id
            };
            using var uow = ApplicationSettings.CreateUnitOfWork();
            uow.QuoteRepository.Add(newQuote);
            uow.Commit();
        }
    }
}
