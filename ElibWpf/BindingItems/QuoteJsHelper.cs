using Domain;

namespace ElibWpf.BindingItems
{
    public class QuoteJsHelper
    {
        private readonly Book book;

        public QuoteJsHelper(Book book)
        {
            this.book = book;
        }

        public async void RegisterQuote(string quote, string note = null)
        {
            var newQuote = new Quote
            {
                Text = quote,
                Note = note,
                BookId = book.Id
            };

            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            uow.QuoteRepository.Add(newQuote);
            uow.Commit();
        }
    }
}
