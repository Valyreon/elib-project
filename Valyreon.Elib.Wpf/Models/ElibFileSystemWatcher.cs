using System;
using System.IO;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Mvvm.Messaging;
using Valyreon.Elib.Wpf.Extensions;
using Valyreon.Elib.Wpf.Messages;

namespace Valyreon.Elib.Wpf.Models
{
    public class ElibFileSystemWatcher : IDisposable
    {
        private readonly ApplicationProperties applicationProperties;
        private readonly IMessenger messenger;
        private readonly IUnitOfWorkFactory uowFactory;
        private readonly FileSystemWatcher watcher = new();

        public ElibFileSystemWatcher(ApplicationProperties applicationProperties, IUnitOfWorkFactory uowFactory, IMessenger messenger = null)
        {
            this.applicationProperties = applicationProperties;
            this.uowFactory = uowFactory;
            this.messenger = messenger;

            if (string.IsNullOrWhiteSpace(applicationProperties.LibraryFolder) || !Directory.Exists(applicationProperties.LibraryFolder))
            {
                return;
            }

            watcher = new FileSystemWatcher(applicationProperties.LibraryFolder)
            {
                Filter = "*.*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Deleted += (_, e) => HandleFileDelete(e.FullPath);
            watcher.Created += (_, e) => HandleFileCreate(e.FullPath);
            watcher.Renamed += (_, e) => HandleFileRename(e.OldFullPath, e.FullPath);
            watcher.Changed += (_, e) => HandleFileChange(e.FullPath);
        }

        public void CheckFiles()
        {
            //IEnumerable<Book> allBooks = null;
        }

        public void Dispose()
        {
            watcher.Dispose();
        }

        private async void HandleFileChange(string filePath)
        {
            if (!applicationProperties.Formats.Contains(filePath.GetExtension()))
            {
                return;
            }

            using var uow = await uowFactory.CreateAsync();
            var book = await uow.BookRepository.GetByPathAsync(filePath);

            if (book == null)
            {
                return;
            }

            await book.LoadBookAsync(uow);
            book.Signature = Signer.ComputeHash(filePath);
            await uow.BookRepository.UpdateAsync(book);
            messenger?.Send(new ShowNotificationMessage($"Book file change detected. Updated signature.", NotificationType.Info));
        }

        private async void HandleFileCreate(string filePath)
        {
            if (!applicationProperties.Formats.Contains(filePath.GetExtension()))
            {
                return;
            }

            // on create calculate signature and see if any signature matches the new file, it means it was moved
            using var uow = await uowFactory.CreateAsync();

            while (filePath.IsFileLocked())
            {
                await Task.Delay(500);
            }

            var signature = Signer.ComputeHash(filePath);

            // maybe it's been moved to another dir
            var book = await uow.BookRepository.GetBySignatureAsync(signature);

            if (book == null || book.Path.Exists())
            {
                return;
            }

            book.Path = filePath;
            book.IsFileMissing = false;
            await uow.BookRepository.UpdateAsync(book);
            uow.Commit();
        }

        private async void HandleFileDelete(string filePath)
        {
            if (!applicationProperties.Formats.Contains(filePath.GetExtension()))
            {
                return;
            }

            // mark file as missing for UI
            using var uow = await uowFactory.CreateAsync();
            var book = await uow.BookRepository.GetByPathAsync(filePath);

            if (book == null)
            {
                return;
            }

            book.IsFileMissing = true;
        }

        private async void HandleFileRename(string oldFilePath, string newFilepath)
        {
            if (!applicationProperties.Formats.Contains(Path.GetExtension(newFilepath)))
            {
                return;
            }

            // on rename update the path
            using var uow = await uowFactory.CreateAsync();
            var book = await uow.BookRepository.GetByPathAsync(oldFilePath);

            if (book == null)
            {
                return;
            }

            book.Path = newFilepath;
            await uow.BookRepository.UpdateAsync(book);
            uow.Commit();
            messenger?.Send(new ShowNotificationMessage($"Book file path updated.", NotificationType.Info));
        }
    }
}
