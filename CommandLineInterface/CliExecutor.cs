using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLineInterface.Utilities;
using DataLayer;
using Domain;
using EbookTools;
using Models;
using Models.Helpers;
using Models.Options;
using Models.Utilities;
using OnlineBookApi;
using OnlineBookApi.OpenLibrary;

namespace Cli
{
    /// <summary>
    ///  This class is used for CLI functionality.
    /// </summary>
    public class CliExecutor : IDisposable
    {
        private readonly ElibContext database;
        private readonly DetailUtils detail;
        private readonly Importer importer;
        private readonly ISet<Book> selectedBooks;

        public CliExecutor()
        {
            this.database = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            this.importer = new Importer(this.database);
            this.selectedBooks = new HashSet<Book>();
            this.detail = new DetailUtils(this.database);
            Console.WriteLine("Starting eLIB in CLI mode.\nWELCOME TO ELIB COMMAND LINE.\n");
        }

        public void Dispose()
        {
            this.database.Dispose();
        }

        public static void Main(string[] args)
        {
            using CliExecutor cliExecutor = new CliExecutor();
            cliExecutor.Execute();
        }

        private bool ConsoleQuestion(string question, bool inverted)
        {
            ConsoleKey consoleKey;

            do
            {
                Console.Write("\n" + question + " " + (inverted ? "[y/N]" : "[Y/n]") + " ");
                consoleKey = Console.ReadKey(true).Key;
            } while (consoleKey != ConsoleKey.Y && consoleKey != ConsoleKey.N && consoleKey != ConsoleKey.Enter);

            Console.Write("\n");

            return consoleKey == ConsoleKey.Y || consoleKey == ConsoleKey.Enter && !inverted;
        }

        private string GetNewOrDefaultInput(string def)
        {
            string newString = Console.ReadLine()?.Trim();

            return string.IsNullOrEmpty(newString) ? def : newString;
        }

        /// <summary>
        ///  Starts the CLI loop until the keyword 'exit' is inputted.
        /// </summary>
        ///
        public void Execute()
        {
            string command;
            do
            {
                Console.Write(">> ");
                var consoleInput = Console.ReadLine().Trim().SplitOnFirstBlank();
                command = consoleInput.Item1.ToLower();
                switch (command)
                {
                    case "":
                        break;

                    case "import":
                    case "i":
                        IEnumerable<string> validFileList = new List<string>();
                        try
                        {
                            if (consoleInput.Item2.EndsWith("*"))// Directory
                            {
                                validFileList = ImportUtils.GetValidFilesFromDirectory(consoleInput.Item2.Substring(0, consoleInput.Item2.Length - 2));
                            }
                            else
                            {
                                // TODO: Check if book exists
                                var fileList = consoleInput.Item2.GetFilePathsFromString();
                                var enumerable = fileList as string[] ?? fileList.ToArray();
                                validFileList = ImportUtils.GetValidBookPaths(enumerable);
                                var invalidFileList = ImportUtils.GetInvalidBookPaths(enumerable);

                                foreach (string invalidFilePath in invalidFileList)
                                {
                                    Console.WriteLine($"File {invalidFilePath} is not valid or does not exist.");
                                }
                            }
                        }
                        catch (DirectoryNotFoundException)
                        {
                            Console.WriteLine("Invalid directory.");
                        }

                        var list = validFileList as string[] ?? validFileList.ToArray();
                        int validFileCount = list.Length;
                        Console.WriteLine($"Found {validFileCount} book{(validFileCount == 1 ? "" : "s")}");

                        Console.WriteLine("\nFound books:");
                        for (int i = 1; i <= validFileCount; i++)
                        {
                            Console.WriteLine($"{i}. {Path.GetFileName(list.ElementAt(i - 1))}");
                        }

                        if (!this.ConsoleQuestion("Do you want to continue?", false))
                        {
                            break;
                        }

                        //Construct book list
                        var parsedBookList = ImportUtils.GetParsedBooksFromPaths(list);
                        IList<Book> newBookList = new List<Book>();

                        foreach (ParsedBook parsedBook in parsedBookList)
                        {
                            Console.WriteLine($"{newBookList.Count() + 1}. {parsedBook.Title}");

                            Console.Write($"Title[{parsedBook.Title}]*: ");
                            string bookName = this.GetNewOrDefaultInput(parsedBook.Title);

                            Console.Write($"Author[{parsedBook.Author}]*: ");
                            string authorName = this.GetNewOrDefaultInput(parsedBook.Author);

                            Console.Write("Series: ");
                            string seriesName = Console.ReadLine().Trim();
                            decimal? seriesNumber = null;

                            if (!string.IsNullOrEmpty(seriesName))
                            {
                                decimal newNumber;
                                do
                                {
                                    Console.Write("Series number: ");
                                } while (!decimal.TryParse(Console.ReadLine().Trim(), out newNumber));

                                seriesNumber = newNumber;
                            }

                            this.importer.ImportBook(parsedBook, bookName, authorName, seriesName, seriesNumber);
                        }
                        break;

                    case "truncate":
                        this.database.TruncateDatabase();
                        break;

                    case "view":
                    case "v":
                        var viewInput = consoleInput.Item2.ToLower().Trim().SplitOnFirstBlank();
                        switch (viewInput.Item1)
                        {
                            case "details":
                            case "d":
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(this.detail.BookDetailsId(id));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Invalid book ID.");
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        Console.WriteLine("Book was not found.");
                                    }
                                }
                                break;

                            case "all":
                            case "a":
                                foreach (int id in this.database.Books.Select(x => x.Id))
                                {
                                    Console.WriteLine(this.detail.BookDetailsId(id));
                                }

                                break;

                            case "author":
                            case "au":
                                if (string.IsNullOrEmpty(viewInput.Item2.Trim()))
                                {
                                    foreach (int id in this.database.Authors.Select(x => x.Id))
                                    {
                                        Console.WriteLine(this.detail.AuthorDetailsId(id));
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(this.detail.AuthorDetailsId(id));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Invalid author ID.");
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        Console.WriteLine("Author was not found.");
                                    }
                                }

                                break;

                            case "collection":
                            case "c":
                                if (string.IsNullOrEmpty(viewInput.Item2.Trim()))
                                {
                                    foreach (int id in this.database.UserCollections.Select(x => x.Id))
                                    {
                                        Console.WriteLine(this.detail.UserCollectionDetailsId(id));
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(this.detail.UserCollectionDetailsId(id));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Invalid User Collection ID.");
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        Console.WriteLine("User Collection was not found.");
                                    }
                                }

                                break;

                            case "series":
                            case "ser":
                                if (string.IsNullOrEmpty(viewInput.Item2.Trim()))
                                {
                                    foreach (int id in this.database.Series.Select(x => x.Id))
                                    {
                                        Console.WriteLine(this.detail.BookSeriesDetailsId(id));
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(this.detail.BookSeriesDetailsId(id));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("Invalid Book Series ID.");
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        Console.WriteLine("Book Series was not found.");
                                    }
                                }

                                break;

                            case "selected":
                            case "s":
                                foreach (Book book in this.selectedBooks)
                                {
                                    Console.Write(this.detail.BookDetailsId(book.Id));
                                }

                                break;

                            default:
                                Console.WriteLine("View command was incorrect");
                                break;
                        }
                        break;

                    case "select":
                    case "s":
                        var selectInput = consoleInput.Item2.ToLower().Trim().SplitOnFirstBlank();
                        switch (selectInput.Item1)
                        {
                            case "book":
                            case "b":
                                var bookIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach (int id in bookIds)
                                {
                                    this.selectedBooks.Add(this.database.Books.Find(id));
                                }

                                break;

                            case "series":
                            case "s":
                                var seriesIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach (int id in seriesIds)
                                {
                                    foreach (Book book in this.database.Series.Include("Books").FirstOrDefault(x => x.Id == id)?.Books)
                                    {
                                        this.selectedBooks.Add(book);
                                    }
                                }

                                break;

                            case "collection":
                            case "c":
                                var collectionIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach (int id in collectionIds)
                                {
                                    foreach (Book book in this.database.UserCollections.Include("Books").FirstOrDefault(x => x.Id == id)?.Books)
                                    {
                                        this.selectedBooks.Add(book);
                                    }
                                }

                                break;

                            case "all":
                            case "a":
                                this.selectedBooks.UnionWith(this.database.Books);
                                break;

                            case "clear":
                            case "clr":
                                this.selectedBooks.Clear();
                                break;

                            case "deselect":
                            case "d":
                                var deselectIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach (int id in deselectIds)
                                {
                                    this.selectedBooks.Remove(this.database.Books.Find(id));
                                }

                                break;

                            default:
                                Console.WriteLine("Select command was incorrect");
                                break;
                        }
                        break;

                    case "export":
                    case "ex":
                        ISet<EFile> exportFiles = new HashSet<EFile>();

                        Console.WriteLine("Selected books for export:");

                        foreach (Book book in this.selectedBooks)
                        {
                            this.database.Entry(book).Collection(f => f.Authors).Load();
                            this.database.Entry(book).Reference(f => f.File).Load();
                            Console.WriteLine($"Id:{book.Id} {book.Title} by {string.Join(", ", book.Authors.Select(x => x.Name))}");
                            //exportFiles.UnionWith(book.Files); TODO: Luka je mjenjao ovo jer je promjenio strukturu baze
                        }

                        int exportChoice = 0;
                        string exportString = "";
                        do
                        {
                            Console.Write("Group exported books by series[0], author(1), both(2), neither(3): ");
                            exportString = Console.ReadLine().Trim();
                        } while (!int.TryParse(exportString, out exportChoice) && (exportChoice == 0 || exportChoice == 1 || exportChoice == 2 || exportChoice == 3));

                        ExporterOptions options = new ExporterOptions();

                        switch (exportChoice)
                        {
                            case 0:
                                options.GroupBySeries = true;
                                break;

                            case 1:
                                options.GroupByAuthor = true;
                                break;

                            case 2:
                                options.GroupByAuthor = options.GroupBySeries = true;
                                break;
                        }

                        options.DestinationDirectory = consoleInput.Item2;
                        //exporter.ExportBooks(exportFiles, options); TODO: Luka commented this out because of a database change, needs to be fixed

                        break;

                    case "delete":
                    case "d":
                        Console.WriteLine("Selected books for deletion:");
                        foreach (Book book in this.selectedBooks)
                        {
                            this.database.Entry(book).Collection(f => f.Authors).Load();
                            this.database.Entry(book).Reference(f => f.File).Load();
                            Console.WriteLine($"Id:{book.Id} {book.Title} by {string.Join(", ", book.Authors.Select(x => x.Name))}");
                        }

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("WARNING");
                        Console.ResetColor();
                        Console.WriteLine(": This is a permanent action. Consider exporting first.");
                        if (this.ConsoleQuestion("Do you want to continue?", true))
                        {
                            this.database.Books.RemoveRange(this.selectedBooks);
                            this.selectedBooks.Clear();
                            this.database.SaveChanges();
                        }
                        break;

                    case "edit":
                    case "ed":/*
                        Console.WriteLine("Selected books for editing:");
                        foreach (Book book in selectedBooks)
                        {
                            database.Entry(book).Reference(f => f.Series).Load();
                            database.Entry(book).Collection(f => f.Authors).Load();
                            Console.WriteLine($"Id:{book.Id} {book.Name} by {(string.Join(", ", book.Authors.Select(x => x.Name)))}");
                        }

                        if (ConsoleQuestion("Do you want to continue?", true))
                        {
                            foreach (Book book in selectedBooks)
                            {
                                Console.Write($"Title[{book.Name}]*: ");
                                book.Name = GetNewOrDefaultInput(book.Name);

                                Console.Write($"Series[{book.Series?.Name}]: ");
                                string seriesName = GetNewOrDefaultInput(book.Series?.Name);
                                decimal? seriesNumber = null;

                                if (!string.IsNullOrEmpty(seriesName))
                                {
                                    decimal newNumber;
                                    do
                                    {
                                        Console.Write("Series number: ");
                                    } while (!decimal.TryParse(Console.ReadLine().Trim(), out newNumber));

                                    seriesNumber = newNumber;
                                }

                                Console.Write($"Read[{book.IsRead}]: ");

                                string isReadInput = Console.ReadLine();

                                try
                                {
                                    book.IsRead = bool.Parse(isReadInput);
                                }
                                catch (FormatException)
                                {
                                }

                                database.Books.AddOrUpdate(book);
                                database.SaveChanges();

                               // Console.Write($"Publisher[{book.Pu}]")
                            }
                        }*/
                        break;

                    case "query":
                    case "q":
                        IOnline online = new OnlineAPI();
                        int localid = int.Parse(consoleInput.Item2);
                        var images = online.GetMultipleCoversAsync(this.database.Books.Include("Authors").Where(x => x.Id == localid).FirstOrDefault()).Result;
                        /*
                        foreach (byte[] img in images)
                        {
                            Image image = Image.FromStream(new MemoryStream(img));
                            image.Save(@"c:\users\gygasync\desktop\testtest\" + img.GetHashCode() + ".jpg", ImageFormat.Jpeg);
                        }
                        */

                        break;

                    case "exit":
                        Console.WriteLine("Exiting...");
                        this.database.SaveChanges();
                        break;

                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            } while (command != "exit");
        }
    }
}