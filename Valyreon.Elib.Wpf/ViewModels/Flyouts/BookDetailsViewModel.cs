using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Valyreon.Elib.Mvvm;using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Domain;
using Valyreon.Elib.EBookTools;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Windows;
using Valyreon.Elib.Wpf.Views.Windows;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private string addCollectionFieldText = "";

        private string bookDescription;

        public BookDetailsViewModel(Book book)
        {
            Book = book;
        }

        public ICommand ReadBookCommand => new RelayCommand(HandleRead);

        private void HandleRead()
        {
            var win2 = new ReaderWindow
            {
                DataContext = new ReaderViewModel(Book)
            };
            win2.Show();
        }

        public ICommand AddCollectionCommand => new RelayCommand<string>(AddCollection);

        public string AddCollectionFieldText
        {
            get => addCollectionFieldText;
            set => Set(() => AddCollectionFieldText, ref addCollectionFieldText, value);
        }

        public Book Book { get; }

        public string BookDescriptionText
        {
            get => bookDescription;
            set => Set(() => BookDescriptionText, ref bookDescription, value);
        }

        public ICommand EditButtonCommand => new RelayCommand(HandleEditButton);

        public ICommand GoToAuthor => new RelayCommand<ICollection<Author>>(a =>
        {
            Messenger.Default.Send(new AuthorSelectedMessage(a.First()));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public ICommand GoToCollectionCommand => new RelayCommand<UserCollection>(GoToCollection);

        public ICommand GoToSeries => new RelayCommand<BookSeries>(a =>
        {
            Messenger.Default.Send(new SeriesSelectedMessage(a));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public bool IsBookFavorite
        {
            get => Book.IsFavorite;
            set
            {
                Book.IsFavorite = value;
                RaisePropertyChanged(() => IsBookFavorite);

                Task.Run(() =>
               {
                   using var uow = App.UnitOfWorkFactory.Create();
                   uow.BookRepository.Update(Book);
                   uow.Commit();
               });
            }
        }

        public bool IsBookRead
        {
            get => Book.IsRead;
            set
            {
                Book.IsRead = value;
                RaisePropertyChanged(() => IsBookRead);

                Task.Run(() =>
                {
                    using var uow = App.UnitOfWorkFactory.Create();
                    uow.BookRepository.Update(Book);
                    uow.Commit();
                });
            }
        }

        public ICommand RemoveCollectionCommand => new RelayCommand<string>(RemoveCollection);

        private void GoToCollection(UserCollection obj)
        {
            MessengerInstance.Send(new CollectionSelectedMessage(obj.Id));
            MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void RemoveCollection(string tag)
        {
            var collection = Book.Collections.FirstOrDefault(c => c.Tag == tag);

            if (collection == null)
            {
                return;
            }

            Book.Collections.Remove(collection);

            Task.Run(() =>
            {
                using var uow = App.UnitOfWorkFactory.Create();
                uow.CollectionRepository.RemoveCollectionForBook(collection, Book.Id);
                if (uow.CollectionRepository.CountBooksInUserCollection(collection.Id) <= 1)
                {
                    uow.CollectionRepository.Remove(collection);
                    MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }
                uow.Commit();
            });
        }

        private void AddCollection(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tag = tag.Trim();
                AddCollectionFieldText = "";
                if (Book.Collections.Any(c => c.Tag == tag)) // check if book is already in that collection
                {
                    return;
                }
                else // if not
                {
                    var newCollection = new UserCollection { Tag = tag };
                    Book.Collections.Add(newCollection);
                    Task.Run(() =>
                    {
                        using var uow = App.UnitOfWorkFactory.Create();
                        var existingCollection = uow.CollectionRepository.GetByTag(tag);
                        if (existingCollection == null)
                        {
                            uow.CollectionRepository.AddCollectionForBook(newCollection, Book.Id);
                            MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                        }
                        else
                        {
                            newCollection.Id = existingCollection.Id;
                            uow.CollectionRepository.AddCollectionForBook(existingCollection, Book.Id);
                        }

                        uow.Commit();
                    });
                }
            }
        }

        private void HandleEditButton()
        {
            MessengerInstance.Send(new EditBookMessage(Book));
        }

        public ICommand ExportButtonCommand => new RelayCommand(HandleExport);

        private async void HandleExport()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Exporter.GenerateName(Book), // Default file name
                DefaultExt = Book.File.Format, // Default file extension
                CheckPathExists = true,
                Title = "Export book file",
                OverwritePrompt = true,
                Filter = $"Book file (*{Book.File.Format})|*{Book.File.Format}"
            };

            var result = dlg.ShowDialog();

            try
            {
                if (result == true)
                {
                    var filePath = dlg.FileName;
                    RawFile fileToExport = null;
                    using (var uow = await App.UnitOfWorkFactory.CreateAsync())
                    {
                        fileToExport = uow.RawFileRepository.Find(Book.File.RawFileId);
                    }

                    Exporter.Export(fileToExport, filePath);
                }
            }
            catch (Exception)
            {
                MessengerInstance.Send(new ShowDialogMessage("Error Notification", "Something went wrong while exporting the file."));
            }
        }

        public ICommand ShowFileInfoCommand => new RelayCommand(HandleShowFileInfo);

        private async void HandleShowFileInfo()
        {
            RawFile rawFile = null;
            using (var uow = await App.UnitOfWorkFactory.CreateAsync())
            {
                rawFile = uow.RawFileRepository.Find(Book.File.RawFileId);
            }

            var x = EbookParserFactory.Create(Book.File.Format, rawFile.RawContent).Parse();

            var builder = new StringBuilder("");
            builder.Append("ISBN: ").AppendLine(x.Isbn);
            builder.Append("Publisher: ").AppendLine(x.Publisher);

            MessengerInstance.Send(new ShowDialogMessage("File Information", builder.ToString()));
        }
    }
}