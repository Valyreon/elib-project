using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.BindingItems
{
    /*public class QuoteJsHelper
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
            await uow.QuoteRepository.CreateAsync(newQuote);
            uow.Commit();
        }
    }*/
}
