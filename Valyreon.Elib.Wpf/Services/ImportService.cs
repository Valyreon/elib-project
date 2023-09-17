using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.Services;

public class ImportService : IImportService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ApplicationProperties appSettings;

    public ImportService(IUnitOfWorkFactory uowFactory, ApplicationProperties appSettings)
    {
        _uowFactory = uowFactory;
        this.appSettings = appSettings;
    }

    public async Task<IReadOnlyList<string>> GetNotImportedBookPathsAsync(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ArgumentException(nameof(path));
        }

        var filesToScan = new List<string>();

        var directories = new Stack<string>();
        directories.Push(path);

        while (directories.Any())
        {
            var currentDir = directories.Pop();
            filesToScan.AddRange(Directory.GetFiles(currentDir));

            foreach (var subDir in Directory.GetDirectories(currentDir))
            {
                directories.Push(subDir);
            }
        }

        var newBooks = new List<string>();

        // only books with correct extension
        foreach (var bookPath in filesToScan.Where(p => appSettings.Formats.Any(f => Path.GetExtension(p).ToLowerInvariant() == f)).ToList())
        {
            var signature = Signer.ComputeHash(bookPath);
            // only books which have not been added already
            using var uow = await _uowFactory.CreateAsync();
            if (!await uow.BookRepository.PathExistsAsync(bookPath) && !await uow.BookRepository.SignatureExistsAsync(signature))
            {
                newBooks.Add(bookPath);
            }
        }

        return newBooks;
    }

    public async Task ImportBookAsync(Book book)
    {
        if (!book.IsValid() || book.Id != 0)
        {
            throw new ArgumentException("Book has to be valid and not imported already.", nameof(book));
        }

        using var uow = await _uowFactory.CreateAsync();
        if (book.Series != null)
        {
            var existingSeries = await uow.SeriesRepository.GetByNameAsync(book.Series.Name);
            if (existingSeries != null)
            {
                book.Series.Id = existingSeries.Id;
                book.SeriesId = existingSeries.Id;
            }
            else
            {
                book.Series.Id = 0;
                await uow.SeriesRepository.CreateAsync(book.Series);
            }
            book.SeriesId = book.Series.Id;
        }

        if (book.Cover != null)
        {
            await uow.CoverRepository.CreateAsync(book.Cover);
            book.CoverId = book.Cover.Id;
        }

        await uow.BookRepository.CreateAsync(book);

        foreach (var author in book.Authors)
        {
            var existingAuthor = await uow.AuthorRepository.GetAuthorWithNameAsync(author.Name);
            if (existingAuthor != null)
            {
                author.Id = existingAuthor.Id;
                await uow.AuthorRepository.AddAuthorForBookAsync(existingAuthor, book.Id);
            }
            else
            {
                author.Id = 0;
                await uow.AuthorRepository.CreateAsync(author);
                await uow.AuthorRepository.AddAuthorForBookAsync(author, book.Id);
            }
        }

        uow.Commit();
    }
}
