using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;

namespace Valyreon.Elib.Wpf.ViewModels.Windows
{
    public class ReaderViewModel : ViewModelBase
    {
        private Book Book { get; }

        public ReaderViewModel(Book book)
        {
            Book = book;
        }

        // public ICommand LoadCommand => new RelayCommand<ChromiumWebBrowser>(HandleLoad);

        // public ICommand UnloadCommand => new RelayCommand<ChromiumWebBrowser>(HandleUnload);

        /*private async void HandleUnload(ChromiumWebBrowser obj)
        {
            var r = obj.EvaluateScriptAsync(@"document.body[""scrollTop""]");
            var scrollTop = (int)r.Result.Result;

            r = obj.EvaluateScriptAsync(@"document.documentElement[""scrollHeight""]");
            var height = (int)r.Result.Result;

            Book.PercentageRead = scrollTop == 0 ? 0 : decimal.Divide(scrollTop, height) * 100m;

            var toUpdate = Book;

            using var uow = await App.UnitOfWorkFactory.CreateAsync();
            uow.BookRepository.Update(toUpdate);
            uow.Commit();
        }*/

        private string bookHtml;

        public string BookHtml
        {
            get => bookHtml;
            set => Set(() => BookHtml, ref bookHtml, value);
        }

        /*private async void HandleLoad(ChromiumWebBrowser obj)
        {
            obj.JavascriptObjectRepository.Register("QuoteHelper", new QuoteJsHelper(Book), true);

            await Task.Run(async () =>
            {
                RawFile rawFile = null;
                using (var uow = await App.UnitOfWorkFactory.CreateAsync())
                {
                    rawFile = uow.RawFileRepository.Find(Book.File.RawFileId);
                }

                var parser = EbookParserFactory.Create(Book.Format, rawFile.RawContent);
                var html = new StringBuilder("<html>" + parser.GenerateHtml());
                if (Book.PercentageRead != 0)
                {
                    html.Append("<script>let scrollPercentage = ").Append(Book.PercentageRead).Append("; document.addEventListener('DOMContentLoaded', function(){document.body[\"scrollTop\"] = document.documentElement[\"scrollHeight\"]* (scrollPercentage / 100);}, false);</script>");
                }

                html.Append("<script>").Append(Resources.JavascriptCode.CtxMenuJS).Append("</script>");
                html.Append("<style>").Append(Resources.JavascriptCode.CtxMenuCss).Append("</style>");
                html.Append(@"<script>// Initialize a context menu for the entire page
                                var contextMenu = CtxMenu();

                                function getSelectionText() {
                                    var text = """";
                                    if (window.getSelection) {
                                        text = window.getSelection().toString();
                                    } else if (document.selection && document.selection.type != ""Control"") {
                                        text = document.selection.createRange().text;
                                    }
                                    return text;
                                }

                                function ContextMenuExampleFunction() {
                                     (async function() {
                                        await CefSharp.BindObjectAsync(""QuoteHelper"");

                                        QuoteHelper.registerQuote(getSelectionText());
                                     })();
                                }

                                // Add our custom function to the menu
                                contextMenu.addItem(""Save Quote"", ContextMenuExampleFunction);
                                </script>");
                html.Append("</html>");

                BookHtml = html.ToString();
            });
        }*/
    }
}
