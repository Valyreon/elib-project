using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.DataLayer.Extensions;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EBookTools;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.BindingItems;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Dialogs;
using Valyreon.Elib.Wpf.Views.Dialogs;

namespace Valyreon.Elib.Wpf.ViewModels.Controls
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
            Filter = filter;
            Books = new ObservableCollection<Book>();
            this.selector = selector;
            ApplyFilterOptionsToFilter(filterOptions, filter);
            UpdateSubcaption();
        }

        public ICommand BackCommand => new RelayCommand(Back);

        public ObservableCollection<Book> Books { get; set; }

        public ICommand GoToAuthor =>
            new RelayCommand<ICollection<Author>>(a => Messenger.Default.Send(new AuthorSelectedMessage(a.First())));

        public ICommand GoToSeries =>
            new RelayCommand<BookSeries>(s => Messenger.Default.Send(new SeriesSelectedMessage(s)));

        public ICommand LoadMoreCommand => new RelayCommand(LoadMore);

        public ICommand OpenBookDetails => new RelayCommand<Book>(HandleBookClick);

        public bool IsBackEnabled => Back != null;

        public ICommand RefreshCommand => new RelayCommand(Refresh);

        public ICommand FilterBooksCommand => new RelayCommand(HandleFilter);

        public ICommand ClearSelectedBooksCommand => new RelayCommand(HandleClearButton);

        public ICommand DeleteSelectedBooksCommand => new RelayCommand(HandleDeleteButton);

        private async void HandleDeleteButton()
        {
            if (ApplicationSettings.GetInstance().IsExportForcedBeforeDelete)
            {
                var choice = await DialogCoordinator.Instance.ShowMessageAsync(
                                    System.Windows.Application.Current.MainWindow.DataContext,
                                    "Confirm Export",
                                    "Books will have to be exported before deleting for security. You can change this in the settings.\n" +
                                    "Do you want to continue?",
                                    MessageDialogStyle.AffirmativeAndNegative,
                                    new MetroDialogSettings { AffirmativeButtonText = "Continue", DefaultButtonFocus = MessageDialogResult.Negative });

                if (choice == MessageDialogResult.Negative)
                {
                    return;
                }
            }

            var dialog = new DeleteBooksDialog();
            IList<Book> selectedBooks = null;
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                selectedBooks = selector.GetSelectedBooks(uow);
            }

            var deleteDialogViewModel = new DeleteBooksDialogViewModel(selectedBooks, dialog);
            deleteDialogViewModel.SetActionOnClose(() =>
            {
                HandleClearButton();
                Messenger.Default.Send(new RefreshSidePaneCollectionsMessage());
            });
            dialog.DataContext = deleteDialogViewModel;
            await DialogCoordinator.Instance.ShowMetroDialogAsync(System.Windows.Application.Current.MainWindow.DataContext, dialog);
        }

        public bool IsSelectedBooksViewer => Filter.Selected == true;

        public bool IsResultEmpty
        {
            get => isResultEmpty;
            set => Set(() => IsResultEmpty, ref isResultEmpty, value);
        }

        private void HandleClearButton()
        {
            selector.Clear();
            Messenger.Default.Send(new ResetPaneSelectionMessage());
            Messenger.Default.Send(new BookSelectedMessage());
        }

        public double ScrollVertical
        {
            get => scrollVerticalOffset;
            set => Set(() => ScrollVertical, ref scrollVerticalOffset, value);
        }

        public ICommand SelectBookCommand => new RelayCommand<Book>(b =>
        {
            b.IsMarked = !b.IsMarked;
            HandleSelectBook(b);
        });

        private FilterParameters filter = null;

        public FilterParameters Filter
        {
            get => filter;
            set
            {
                filter = value;
                if (Books != null)
                {
                    Refresh();
                }
            }
        }

        public string Caption
        {
            get => caption;
            set => Set(() => Caption, ref caption, value);
        }

        private string subCaption;

        public string SubCaption
        {
            get => subCaption;
            set => Set(() => SubCaption, ref subCaption, value);
        }

        public int CurrentCount => Books.Count;

        private void HandleSelectBook(Book obj)
        {
            var isSelected = selector.Select(obj);
            var isThisSelectedView = Filter.Selected == true;
            if (isThisSelectedView && !isSelected && Books.Count == 1)
            {
                MessengerInstance.Send(new ResetPaneSelectionMessage());
            }
            else if (isThisSelectedView && !isSelected && Books.Count > 1)
            {
                Books.Remove(obj);
                UpdateSubcaption();
            }
            else
            {
                lastSelectedBook = obj;
            }

            MessengerInstance.Send(new BookSelectedMessage());
        }

        private void HandleBookClick(Book arg)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                arg.IsMarked = !arg.IsMarked;
                HandleSelectBook(arg);
                RaisePropertyChanged(() => arg.IsMarked);
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var lastSelectedIndex = lastSelectedBook == null ? 0 : Books.IndexOf(lastSelectedBook);
                var currentIndex = Books.IndexOf(arg);
                var ascIndexArray = new int[2];
                ascIndexArray[0] = currentIndex > lastSelectedIndex ? lastSelectedIndex : currentIndex;
                ascIndexArray[1] = currentIndex > lastSelectedIndex ? currentIndex : lastSelectedIndex;

                for (var i = ascIndexArray[0]; i <= ascIndexArray[1]; i++)
                {
                    var book = Books.ElementAt(i);
                    book.IsMarked = true;
                    HandleSelectBook(book);
                    RaisePropertyChanged(() => book.IsMarked);
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
            {
                return;
            }

            await semaphore.WaitAsync();
            var callingMethod = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
            Console.WriteLine(callingMethod);

            if (dontLoad)
            {
                return;
            }

            await Task.Factory.StartNew(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();
                if (filter.Selected.HasValue && filter.Selected == true)
                {
                    dontLoad = true;
                    return uow.BookRepository.GetBooks(selector.SelectedIds).ToList();
                }
                else
                {
                    return uow.BookRepository.FindPageByFilter(filter, Books.Count, 25).ToList();
                }
            }).ContinueWith(x =>
            {
                using var uow = App.UnitOfWorkFactory.Create();

                if (x.Result.Count == 0)
                {
                    IsResultEmpty = Books.Count == 0;
                    return;
                }

                foreach (var item in x.Result)
                {
                    Books.Add(selector.SetMarked(item).LoadMembers(uow));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            _ = semaphore.Release();
        }

        public void Refresh()
        {
            ScrollVertical = 0;
            Books.Clear();
            UnitOfWork.ClearCache();
            UpdateSubcaption();
        }

        public async Task<IViewer> Search(SearchParameters searchOptions)
        {
            var filterWithSearch = filter.Clone();

            if (!string.IsNullOrEmpty(searchOptions.Token))
            {
                filterWithSearch.SearchParameters = searchOptions.Clone();
            }
            else
            {
                return null;
            }

            var resultCount = 0;
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                resultCount = uow.BookRepository.Count(filterWithSearch);
            }

            if (resultCount == 0)
            {
                return null;
            }

            var res = new BookViewerViewModel(filterWithSearch, selector)
            {
                Caption = $"Search results for '{searchOptions.Token}' in {Caption}"
            };

            return res;
        }

        private async void HandleFilter()
        {
            var dialog = new FilterOptionsDialog
            {
                DataContext = new FilterOptionsDialogViewModel(filterOptions, f =>
                {
                    filterOptions = f;
                    ApplyFilterOptionsToFilter(f, filter);
                    Refresh();
                    LoadMore();
                })
            };
            await DialogCoordinator.Instance.ShowMetroDialogAsync(System.Windows.Application.Current.MainWindow.DataContext, dialog);
        }

        public ICommand ExportSelectedBooksCommand => new RelayCommand(HandleExport);

        private async void HandleExport()
        {
            var dialog = new ExportOptionsDialog();
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                dialog.DataContext = new ExportOptionsDialogViewModel(selector.GetSelectedBooks(uow), dialog);
            }
            await DialogCoordinator.Instance.ShowMetroDialogAsync(System.Windows.Application.Current.MainWindow.DataContext, dialog);
        }

        public ICommand AddBookCommand => new RelayCommand(ProcessAddBook);

        private async void ProcessAddBook()
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Epub files|*.epub|Mobi files|*.mobi|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 3,
                Multiselect = true
            };
            var result = dlg.ShowDialog();
            if (result == DialogResult.OK && dlg.FileNames.Length > 0)
            {
                var booksToAdd = new List<Book>();
                var controller =
                    await DialogCoordinator.Instance.ShowProgressAsync(System.Windows.Application.Current.MainWindow.DataContext,
                        "Please wait...", "");
                controller.Maximum = dlg.FileNames.Length;
                controller.Minimum = 1;
                for (var i = 0; i < dlg.FileNames.Length; i++)
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
                            var pBook = EbookParserFactory.Create(dlg.FileNames[i]).Parse();
                            using var uow = App.UnitOfWorkFactory.Create();
                            var book = pBook.ToBook(uow);
                            booksToAdd.Add(book);
                        });
                    }
                    catch (Exception)
                    {
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
                MessengerInstance.Send(new OpenAddBooksFormMessage(booksToAdd));
            }
        }

        private void ApplyFilterOptionsToFilter(FilterOptions f, FilterParameters p)
        {
            if (f != null && p != null)
            {
                if (f.ShowAll)
                {
                    p.Read = null;
                }
                else if (f.ShowRead)
                {
                    p.Read = true;
                }
                else if (f.ShowUnread)
                {
                    p.Read = false;
                }

                p.SortByAuthor = f.SortByAuthor;
                p.SortByImportOrder = f.SortByImportTime;
                p.SortBySeries = f.SortBySeries;
                p.SortByTitle = f.SortByTitle;
                p.Ascending = f.Ascending;
            }
        }

        public void Clear()
        {
            Books.Clear();
            ScrollVertical = 0;
        }

        private void UpdateSubcaption()
        {
            _ = Task.Run(() =>
            {
                var count = 0;
                if (!IsSelectedBooksViewer)
                {
                    using var uow = App.UnitOfWorkFactory.Create();
                    count = uow.BookRepository.Count(filter);
                }
                else
                {
                    count = selector.Count;
                }

                SubCaption = $"{count} book";
                if (count != 1)
                {
                    SubCaption += "s";
                }

                if (Filter.Read != null)
                {
                    SubCaption += ", filter is applied";
                }
            });
        }
    }
}
