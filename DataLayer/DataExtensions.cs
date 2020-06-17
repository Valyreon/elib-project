using Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public static class DataExtensions
    {
        public static void AddOrUpdate(this UnitOfWork unitOfWork, Book book)
        {
            if(book.Id == 0) // insert
            {
                if (book.File == null && book.FileId == 0)
                    throw new ArgumentException("Book has to have a file.");
                else if(book.File.Id == 0)
                {
                    if (book.File.RawFile == null && book.File.RawFileId == 0)
                        throw new ArgumentException("EFile has to have RawFile connected.");
                    else if(book.File.RawFile.Id == 0)
                    {
                    }
                }
            }
            else // update
            {

            }
        }

        public static Book LoadMembers(this Book book, UnitOfWork uow)
        {
            if(book.SeriesId.HasValue)
                book.Series = uow.SeriesRepository.Find(book.SeriesId.Value);
            book.File = uow.EFileRepository.Find(book.FileId);
            book.Collections = new ObservableCollection<UserCollection>(uow.CollectionRepository.GetUserCollectionsOfBook(book.Id));
            book.Authors = new ObservableCollection<Author>(uow.AuthorRepository.GetAuthorsOfBook(book.Id));
            return book;
        }
    }
}
