using Domain;
using EbookTools;
using ElibWpf.Messages;
using ElibWpf.ViewModels.Windows;
using ElibWpf.Views.Windows;
using Models;
using MVVMLibrary;
using MVVMLibrary.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private string addCollectionFieldText = "";

        private string bookDescription;

        public BookDetailsViewModel(Book book)
        {
            this.Book = book;
        }

        public ICommand ReadBookCommand => new RelayCommand(this.HandleRead);

        private void HandleRead()
        {
            ReaderWindow win2 = new ReaderWindow
            {
                DataContext = new ReaderViewModel(this.Book)
            };
            win2.Show();
        }

        public ICommand AddCollectionCommand => new RelayCommand<string>(this.AddCollection);

        public string AddCollectionFieldText
        {
            get => this.addCollectionFieldText;
            set => base.Set(() => this.AddCollectionFieldText, ref this.addCollectionFieldText, value);
        }

        public Book Book { get; }

        public string BookDescriptionText
        {
            get => this.bookDescription;
            set => this.Set(() => BookDescriptionText, ref this.bookDescription, value);
        }

        public ICommand EditButtonCommand => new RelayCommand(this.HandleEditButton);

        public ICommand GoToAuthor => new RelayCommand<ICollection<Author>>(a =>
        {
            Messenger.Default.Send(new AuthorSelectedMessage(a.First()));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public ICommand GoToCollectionCommand => new RelayCommand<UserCollection>(this.GoToCollection);

        public ICommand GoToSeries => new RelayCommand<BookSeries>(a =>
        {
            Messenger.Default.Send(new SeriesSelectedMessage(a));
            Messenger.Default.Send(new CloseFlyoutMessage());
        });

        public bool IsBookFavorite
        {
            get => this.Book.IsFavorite;
            set
            {
                this.Book.IsFavorite = value;
                this.RaisePropertyChanged(() => this.IsBookFavorite);

                Task.Run(() =>
                {
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.BookRepository.Update(this.Book);
                    uow.Commit();
                });
            }
        }

        public bool IsBookRead
        {
            get => this.Book.IsRead;
            set
            {
                this.Book.IsRead = value;
                this.RaisePropertyChanged(() => this.IsBookRead);

                Task.Run(() =>
                {
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    uow.BookRepository.Update(this.Book);
                    uow.Commit();
                });
            }
        }

        public ICommand LoadOnlineApiCommand => new RelayCommand(this.LoadOnlineApiAsync);

        public ICommand RemoveCollectionCommand => new RelayCommand<string>(this.RemoveCollection);

        private void GoToCollection(UserCollection obj)
        {
            this.MessengerInstance.Send(new CollectionSelectedMessage(obj.Id));
            this.MessengerInstance.Send(new CloseFlyoutMessage());
        }

        private void RemoveCollection(string tag)
        {
            UserCollection collection = this.Book.Collections.FirstOrDefault(c => c.Tag == tag);

            if (collection == null)
                return;

            this.Book.Collections.Remove(collection);

            Task.Run(() =>
            {
                using var uow = ApplicationSettings.CreateUnitOfWork();
                uow.CollectionRepository.RemoveCollectionForBook(collection, this.Book.Id);
                if (uow.CollectionRepository.CountBooksInUserCollection(collection.Id) <= 1)
                {
                    uow.CollectionRepository.Remove(collection);
                    this.MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                }
                uow.Commit();
            });
        }

        private void AddCollection(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tag = tag.Trim();
                this.AddCollectionFieldText = "";
                if (this.Book.Collections.Any(c => c.Tag == tag)) // check if book is already in that collection
                {
                    return;
                }
                else // if not
                {
                    UserCollection newCollection = new UserCollection { Tag = tag };
                    this.Book.Collections.Add(newCollection);
                    Task.Run(() =>
                    {
                        using var uow = ApplicationSettings.CreateUnitOfWork();
                        UserCollection existingCollection = uow.CollectionRepository.GetByTag(tag);
                        if (existingCollection == null)
                        {
                            uow.CollectionRepository.AddCollectionForBook(newCollection, this.Book.Id);
                            this.MessengerInstance.Send(new RefreshSidePaneCollectionsMessage());
                        }
                        else
                        {
                            newCollection.Id = existingCollection.Id;
                            uow.CollectionRepository.AddCollectionForBook(existingCollection, this.Book.Id);
                        }

                        uow.Commit();
                    });
                }
            }
        }

        private void LoadOnlineApiAsync()
        {
            // fill here stuff from online api
        }

        private void HandleEditButton()
        {
            this.MessengerInstance.Send(new EditBookMessage(this.Book));
        }

        public ICommand ExportButtonCommand { get => new RelayCommand(this.HandleExport); }

        private void HandleExport()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Exporter.GenerateName(Book), // Default file name
                DefaultExt = Book.File.Format, // Default file extension
                CheckPathExists = true,
                Title = "Export book file",
                OverwritePrompt = true,
                Filter = $"Book file (*{Book.File.Format})|*{Book.File.Format}"
            };

            bool? result = dlg.ShowDialog();

            try
            {
                if (result == true)
                {
                    string filePath = dlg.FileName;
                    using var uow = ApplicationSettings.CreateUnitOfWork();
                    Exporter exporter = new Exporter(uow);
                    RawFile fileToExport = uow.RawFileRepository.Find(Book.File.RawFileId);
                    exporter.Export(fileToExport, filePath);
                }
            }
            catch (Exception)
            {
                MessengerInstance.Send(new ShowDialogMessage("Error Notification", "Something went wrong while exporting the file."));
            }
        }

        public ICommand ShowFileInfoCommand { get => new RelayCommand(this.HandleShowFileInfo); }

        private void HandleShowFileInfo()
        {
            using var uow = ApplicationSettings.CreateUnitOfWork();
            RawFile fileToExport = uow.RawFileRepository.Find(Book.File.RawFileId);

            ParsedBook x = EbookParserFactory.Create(Book.File.Format, fileToExport.RawContent).Parse();

            StringBuilder builder = new StringBuilder("");
            builder.AppendLine($"ISBN: {x.Isbn}");
            builder.AppendLine($"Publisher: {x.Publisher}");

            MessengerInstance.Send(new ShowDialogMessage("File Information", builder.ToString()));
        }
    }
}