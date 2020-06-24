using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DataLayer;
using Domain;
using EbookTools;
using ElibWpf.BindingItems;
using ElibWpf.Extensions;
using ElibWpf.Messages;
using ElibWpf.ViewModels.Dialogs;
using ElibWpf.Views.Dialogs;
using MahApps.Metro.Controls.Dialogs;
using Models;
using MVVMLibrary;
using MVVMLibrary.Messaging;

namespace ElibWpf.ViewModels.Controls
{
    public class BookViewerViewModel : ViewModelBase, IViewer
    {
        private readonly Selector selector;
        private string caption;
        private Book lastSelectedBook;
        private double scrollVerticalOffset;
        private bool dontLoad = false;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static FilterOptions filterOptions = new FilterOptions();
        private bool isResultEmpty = false;

        private Action backAction;
        public Action Back
        {
            get => backAction;
            set
            {
                Set(() => Back, ref backAction, value);
                RaisePropertyChanged(() => IsBackEnabled);
            }
        }

        public BookViewerViewModel(FilterParameters filter, Selector selector)
        {
            this.Filter = filter;
            this.Books = new ObservableCollection<Book>();
            this.selector = selector;
            ApplyFilterOptionsToFilter(filterOptions, filter);
            UpdateSubcaption();
        }

        public ICommand BackCommand => new RelayCommand(this.Back);

        public ObservableCollection<Book> Books { get; set; }

        public ICommand GoToAuthor =>
            new RelayCommand<ICollection<Author>>(a => Messenger.Default.Send(new AuthorSelectedMessage(a.First())));

        public ICommand GoToSeries =>
            new RelayCommand<BookSeries>(s => Messenger.Default.Send(new SeriesSelectedMessage(s)));

        public ICommand LoadMoreCommand => new RelayCommand(this.LoadMore);

        public ICommand OpenBookDetails => new RelayCommand<Book>(this.HandleBookClick);

        public bool IsBackEnabled { get => Back != null; }

        public ICommand RefreshCommand => new RelayCommand(this.Refresh);

        public ICommand FilterBooksCommand => new RelayCommand(this.HandleFilter);

        public ICommand ClearSelectedBooksCommand => new RelayCommand(this.HandleClearButton);

        public bool IsSelectedBooksViewer => this.Filter.Selected != null && this.Filter.Selected.Value == true;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        private void HandleClearButton()
        {
            this.selector.Clear();
            this.Refresh();
        }

        public double ScrollVertical
        {
            get => this.scrollVerticalOffset;
            set => this.Set(() => ScrollVertical, ref this.scrollVerticalOffset, value);
        }

        public ICommand SelectBookCommand => new RelayCommand<Book>(b =>
        {
            b.IsMarked = !b.IsMarked;
            this.HandleSelectBook(b);
        });

        private FilterParameters filter = null;
        public FilterParameters Filter
        {
            get => filter;
            set
            {
                filter = value;
                if (Books != null)
                    this.Refresh();
            }
        }

        public string Caption
        {
            get => this.caption;
            set => this.Set(() => Caption, ref this.caption, value);
        }

        private string subCaption;
        public string SubCaption
        {
            get => this.subCaption;
            set => this.Set(() => SubCaption, ref this.subCaption, value);
        }

        public int CurrentCount => Books.Count;

        private void HandleSelectBook(Book obj)
        {
            bool isSelected = this.selector.Select(obj);
            bool isThisSelectedView = Filter.Selected.HasValue && Filter.Selected.Value;
            if (isThisSelectedView && !isSelected && this.Books.Count == 1)
            {
                this.MessengerInstance.Send(new ResetPaneSelectionMessage());
            }
            else if (isThisSelectedView && !isSelected && this.Books.Count > 1)
            {
                this.Books.Remove(obj);
            }
            else
            {
                this.lastSelectedBook = obj;
            }

            this.MessengerInstance.Send(new BookSelectedMessage());
        }

        private void HandleBookClick(Book arg)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                arg.IsMarked = !arg.IsMarked;
                this.HandleSelectBook(arg);
                this.RaisePropertyChanged(() => arg.IsMarked);
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                int lastSelectedIndex = this.lastSelectedBook == null ? 0 : this.Books.IndexOf(this.lastSelectedBook);
                int currentIndex = this.Books.IndexOf(arg);
                var ascIndexArray = new int[2];
                ascIndexArray[0] = currentIndex > lastSelectedIndex ? lastSelectedIndex : currentIndex;
                ascIndexArray[1] = currentIndex > lastSelectedIndex ? currentIndex : lastSelectedIndex;

                for (int i = ascIndexArray[0]; i <= ascIndexArray[1]; i++)
                {
                    Book book = this.Books.ElementAt(i);
                    book.IsMarked = true;
                    this.HandleSelectBook(book);
                    this.RaisePropertyChanged(() => book.IsMarked);
                }
            }
            else
            {
                Messenger.Default.Send(new ShowBookDetailsMessage(arg));
            }
        }

        private async void LoadMore()
        {
            if (semaphore.CurrentCount == 0)
                return;

            await semaphore.WaitAsync();
            string callingMethod = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
            Console.WriteLine(callingMethod);

            if (dontLoad) return;

            await Task.Factory.StartNew(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                if (filter.Selected.HasValue && filter.Selected == true)
                {
                    dontLoad = true;
                    return uow.BookRepository.GetBooks(selector.SelectedIds).ToList();
                }
                else
                    return uow.BookRepository.FindPageByFilter(filter, Books.Count, 25).ToList();
            }).ContinueWith((x) =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();

                if (x.Result.Count == 0)
                {
                    this.IsResultEmpty = this.Books.Count == 0;
                    return;
                }

                foreach (Book item in x.Result)
                {
                    this.Books.Add(this.selector.SetMarked(item).LoadMembers(uow));
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
            semaphore.Release();
        }

        public void Refresh()
        {
            this.ScrollVertical = 0;
            this.Books.Clear();
            using var uow = ApplicationSettings.CreateUnitOfWork();
            uow.ClearCache();
            uow.Dispose();
            this.UpdateSubcaption();
        }

        public IViewer Search(SearchParameters searchOptions)
        {
            var filterWithSearch = this.filter.Clone();

            if (!string.IsNullOrEmpty(searchOptions.Token))
            {
                filterWithSearch.SearchParameters = searchOptions.Clone();
            }
            else return null;

            using var uow = ApplicationSettings.CreateUnitOfWork();
            int resultCount = uow.BookRepository.Count(filterWithSearch);

            if (resultCount == 0)
                return null;

            var res = new BookViewerViewModel(filterWithSearch, selector)
            {
                Caption = $"Search results for '{searchOptions.Token}' in {this.Caption}"
            };

            return res;
        }

        private async void HandleFilter()
        {
            FilterOptionsDialog dialog = new FilterOptionsDialog();
            dialog.DataContext = new FilterOptionsDialogViewModel(filterOptions, f =>
            {
                filterOptions = f;
                ApplyFilterOptionsToFilter(f, filter);
                this.Refresh();
                this.LoadMore();
            });
            await DialogCoordinator.Instance.ShowMetroDialogAsync(App.Current.MainWindow.DataContext, dialog);
        }

        public ICommand ExportSelectedBooksCommand => new RelayCommand(this.HandleExport);

        private async void HandleExport()
        {
            ExportOptionsDialog dialog = new ExportOptionsDialog();
            using var uow = ApplicationSettings.CreateUnitOfWork();
            dialog.DataContext = new ExportOptionsDialogViewModel(this.selector.GetSelectedBooks(uow), dialog);
            await DialogCoordinator.Instance.ShowMetroDialogAsync(App.Current.MainWindow.DataContext, dialog);
        }

        public ICommand AddBookCommand => new RelayCommand(this.ProcessAddBook);

        private async void ProcessAddBook()
        {
            using OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Epub files|*.epub|Mobi files|*.mobi|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 3,
                Multiselect = true
            };
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileNames.Any())
            {
                var booksToAdd = new List<Book>();
                ProgressDialogController controller =
                    await DialogCoordinator.Instance.ShowProgressAsync(App.Current.MainWindow.DataContext,
                        "Please wait...", "");
                controller.Maximum = dlg.FileNames.Length;
                controller.Minimum = 1;
                for (int i = 0; i < dlg.FileNames.Length; i++)
                {
                    await Task.Run(() =>
                    {
                        controller.SetMessage($"Parsing book: {i + 1}");
                        controller.SetProgress(i + 1);
                    });
                    try
                    {
                        await Task.Run(() =>
                        {
                            ParsedBook pBook = EbookParserFactory.Create(dlg.FileNames[i]).Parse();
                            using var uow = ApplicationSettings.CreateUnitOfWork();
                            Book book = pBook.ToBook(uow);
                            booksToAdd.Add(book);
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Log("BOOK_PARSE_ERROR", $"\nMESSAGE:{e.Message}\nSTACK:{e.StackTrace}");
                        var content = File.ReadAllBytes(dlg.FileNames[i]);
                        booksToAdd.Add(new Book
                        {
                            Collections = new ObservableCollection<UserCollection>(),
                            File = new EFile
                            {
                                Format = Path.GetExtension(dlg.FileNames[i]),
                                Signature = Signer.ComputeHash(content),
                                RawFile = new RawFile { RawContent = content }
                            },
                            Authors = new ObservableCollection<Author>()
                        });
                    }
                }

                await controller.CloseAsync();
                this.MessengerInstance.Send(new OpenAddBooksFormMessage(booksToAdd));
            }
        }

        private void ApplyFilterOptionsToFilter(FilterOptions f, FilterParameters p)
        {
            if (f != null && p != null)
            {
                if (f.ShowAll) p.Read = null;
                else if (f.ShowRead) p.Read = true;
                else if (f.ShowUnread) p.Read = false;

                p.SortByAuthor = f.SortByAuthor;
                p.SortByImportOrder = f.SortByImportTime;
                p.SortBySeries = f.SortBySeries;
                p.SortByTitle = f.SortByTitle;
                p.Ascending = f.Ascending;
            }
        }

        public void Clear()
        {
            this.Books.Clear();
            this.ScrollVertical = 0;
        }

        private void UpdateSubcaption()
        {
            _ = Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                int count = uow.BookRepository.Count(filter);
                SubCaption = $"{count} book";
                if (count != 1)
                    SubCaption += "s";
                if (this.Filter.Read != null)
                    SubCaption += ", filter is applied";
            });
        }
    }
}