using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.Services;
public class ImportService : IImportService
{
    private readonly IUnitOfWork _uow;

    public ImportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<string>> ImportAsync()
    {
        var appSettings = ApplicationData.GetProperties();
        var newBooks = new List<string>();

        foreach(var sourcePath in appSettings.Sources)
        {
            newBooks.AddRange(await ImportAsync(sourcePath));
        }

        return newBooks;
    }

    public async Task<IEnumerable<string>> ImportAsync(SourcePath sourcePath)
    {
        if (!Directory.Exists(sourcePath.Path))
        {
            throw new ArgumentException(nameof(sourcePath.Path));
        }

        var filesToScan = new List<string>();

        var directories = new Stack<string>();
        directories.Push(sourcePath.Path);

        while (directories.Any())
        {
            var currentDir = directories.Pop();
            filesToScan.AddRange(Directory.GetFiles(currentDir));

            if (sourcePath.RecursiveScan)
            {
                var subDirs = Directory.GetDirectories(currentDir);

                foreach (var subDir in subDirs)
                {
                    directories.Push(subDir);
                }
            }
        }

        var newBooks = new List<string>();
        var appSettings = ApplicationData.GetProperties();
        var extensions = filesToScan.Select(p => Path.GetExtension(p).ToLowerInvariant()).ToList();
        var filesWithFilteredExtension = filesToScan.Where(p => appSettings.Formats.Any(f => Path.GetExtension(p).ToLowerInvariant() == f)).ToList();
        // only books with correct extension
        foreach (var bookPath in filesWithFilteredExtension)
        {
            // only books which have not been added already
            if (!await _uow.BookRepository.PathExistsAsync(bookPath))
            {
                newBooks.Add(bookPath);
            }
        }

        return newBooks;
    }
}
