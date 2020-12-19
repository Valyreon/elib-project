﻿using CefSharp;
using CefSharp.Wpf;
using Domain;
using EbookTools;
using ElibWpf.BindingItems;
using ElibWpf.Models;
using MVVMLibrary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Windows
{
    public class ReaderViewModel : ViewModelBase
    {
        private Book Book { get; set; }

        public ReaderViewModel(Book book)
        {
            Book = book;
        }

        public ICommand LoadCommand => new RelayCommand<ChromiumWebBrowser>(HandleLoad);

        public ICommand UnloadCommand => new RelayCommand<ChromiumWebBrowser>(HandleUnload);

        private void HandleUnload(ChromiumWebBrowser obj)
        {
            var r = obj.EvaluateScriptAsync(@"document.body[""scrollTop""]");
            var scrollTop = (int)r.Result.Result;

            r = obj.EvaluateScriptAsync(@"document.documentElement[""scrollHeight""]");
            var height = (int)r.Result.Result;

            Book.PercentageRead = scrollTop == 0 ? 0 : decimal.Divide(scrollTop, height) * 100m;

            var toUpdate = Book;

            using var uow = ApplicationSettings.CreateUnitOfWork();
            uow.BookRepository.Update(toUpdate);
            uow.Commit();
        }

        private string bookHtml;

        public string BookHtml
        {
            get => bookHtml;
            set => Set(() => BookHtml, ref bookHtml, value);
        }

        private async void HandleLoad(ChromiumWebBrowser obj)
        {
            obj.JavascriptObjectRepository.Register("QuoteHelper", new QuoteJsHelper(Book), true);

            await Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                var rawFile = uow.RawFileRepository.Find(Book.File.RawFileId);
                var parser = EbookParserFactory.Create(Book.File.Format, rawFile.RawContent);
                var html = new StringBuilder("<html>" + parser.GenerateHtml());
                if (Book.PercentageRead != 0)
                {
                    html.Append($"<script>let scrollPercentage = {Book.PercentageRead}; document.addEventListener('DOMContentLoaded', function(){{document.body[\"scrollTop\"] = document.documentElement[\"scrollHeight\"]* (scrollPercentage / 100);}}, false);</script>");
                }

                html.Append($"<script>{Resources.JavascriptCode.CtxMenuJS}</script>");
                html.Append($"<style>{Resources.JavascriptCode.CtxMenuCss}</style>");
                html.Append(@$"<script>// Initialize a context menu for the entire page
                                var contextMenu = CtxMenu();

                                function getSelectionText() {{
                                    var text = """";
                                    if (window.getSelection) {{
                                        text = window.getSelection().toString();
                                    }} else if (document.selection && document.selection.type != ""Control"") {{
                                        text = document.selection.createRange().text;
                                    }}
                                    return text;
                                }}

                                function ContextMenuExampleFunction() {{
                                     (async function() {{
                                        await CefSharp.BindObjectAsync(""QuoteHelper"");

                                        QuoteHelper.registerQuote(getSelectionText());
                                     }})();
                                }}

                                // Add our custom function to the menu
                                contextMenu.addItem(""Save Quote"", ContextMenuExampleFunction);
                                </script>");
                html.Append("</html>");

                BookHtml = html.ToString();
            });
        }
    }
}