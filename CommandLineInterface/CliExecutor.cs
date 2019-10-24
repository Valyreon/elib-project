using CommandLineInterface.Utilities;
using DataLayer;
using Domain;
using EbookTools;
using Models;
using Models.Helpers;
using Models.Options;
using Models.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cli
{
    /// <summary>
    ///  This class is used for CLI functionality.
    /// </summary>
    public class CliExecutor : IDisposable
    {
        private ElibContext database;
        private Importer importer;
        private Exporter exporter;
        private DetailUtils detail;
        private ISet<Book> selectedBooks;

        public static void Main(string[] args)
        {
            using CliExecutor cliExecutor = new CliExecutor();
            cliExecutor.Execute();
        }

        public CliExecutor()
        {
            database = new ElibContext(ApplicationSettings.GetInstance().DatabasePath);
            importer = new Importer(database);
            exporter = new Exporter(database);
            selectedBooks = new HashSet<Book>();
            detail = new DetailUtils(database);
            Console.WriteLine("Starting eLIB in CLI mode.\nWELCOME TO ELIB COMMAND LINE.\n");
        }

        private bool ConsoleQuestion(string question, bool inverted)
        {
            ConsoleKey consoleKey;

            do
            {
                Console.Write("\n" + question + " " + (inverted ? "[y/N]" : "[Y/n]") + " ");
                consoleKey = Console.ReadKey(false).Key;
            } while (consoleKey != ConsoleKey.Y && consoleKey != ConsoleKey.N && consoleKey != ConsoleKey.Enter);

            Console.Write("\n");

            return consoleKey == ConsoleKey.Y || consoleKey == ConsoleKey.Enter && !inverted;
        }

        private string GetNewOrDefaultInput(string def)
        {
            string newString = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(newString))
                return def;

            return newString;
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
                Tuple<string, string> consoleInput = Console.ReadLine().Trim().SplitOnFirstBlank();
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
                                IEnumerable<string> fileList = consoleInput.Item2.GetFilePathsFromString();
                                validFileList = ImportUtils.GetValidBookPaths(fileList);
                                IEnumerable<string> invalidFileList = ImportUtils.GetInvalidBookPaths(fileList);

                                foreach (string invalidFilePath in invalidFileList)
                                    Console.WriteLine($"File {invalidFilePath} is not valid or does not exist.");
                            }
                        }
                        catch (DirectoryNotFoundException dnfe)
                        {
                            Console.WriteLine("Invalid directory.");
                        }

                        int validFileCount = validFileList.Count();
                        Console.WriteLine($"Found {validFileCount} book{(validFileCount == 1 ? "" : "s")}");

                        Console.WriteLine("\nFound books:");
                        for (int i = 1; i <= validFileCount; i++)
                            Console.WriteLine($"{i}. {Path.GetFileName(validFileList.ElementAt(i - 1))}");

                        if (!ConsoleQuestion("Do you want to continue?", false))
                            break;

                        //Construct book list
                        IEnumerable<ParsedBook> parsedBookList = ImportUtils.GetParsedBooksFromPaths(validFileList);
                        IList<Book> newBookList = new List<Book>();

                        foreach (ParsedBook parsedBook in parsedBookList)
                        {
                            Console.WriteLine($"{newBookList.Count() + 1}. {parsedBook.Title}");

                            Console.Write($"Title[{parsedBook.Title}]*: ");
                            string bookName = GetNewOrDefaultInput(parsedBook.Title);

                            Console.Write($"Author[{parsedBook.Author}]*: ");
                            string authorName = GetNewOrDefaultInput(parsedBook.Author);

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

                            importer.ImportBook(parsedBook, bookName, authorName, seriesName, seriesNumber);
                        }
                break;

                    case "truncate":
                        database.TruncateDatabase();
                        break;

                    case "view":
                    case "v":
                        Tuple<string, string> viewInput = consoleInput.Item2.ToLower().Trim().SplitOnFirstBlank();
                        switch (viewInput.Item1)
                        {
                            case "details":
                            case "d":
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(detail.BookDetailsID(id));
                                    }
                                    catch (FormatException fe)
                                    {
                                        Console.WriteLine("Invalid book ID.");
                                    }
                                    catch (KeyNotFoundException knfe)
                                    {
                                        Console.WriteLine("Book was not found.");
                                    }
                                }
                                break;

                            case "all":
                            case "a":
                                foreach (int id in database.Books.Select(x => x.Id))
                                {
                                    Console.WriteLine(detail.BookDetailsID(id));
                                }
                                break;

                            case "author":
                            case "au":
                                if (string.IsNullOrEmpty(viewInput.Item2.Trim()))
                                {
                                    foreach (int id in database.Authors.Select(x => x.Id))
                                    {
                                        Console.WriteLine(detail.AuthorDetailsID(id));
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(detail.AuthorDetailsID(id));
                                    }
                                    catch (FormatException fe)
                                    {
                                        Console.WriteLine("Invalid author ID.");
                                    }
                                    catch (KeyNotFoundException knfe)
                                    {
                                        Console.WriteLine("Author was not found.");
                                    }
                                }
                                break;
                            case "collection":
                            case "c":
                                if (string.IsNullOrEmpty(viewInput.Item2.Trim()))
                                {
                                    foreach (int id in database.UserCollections.Select(x => x.Id))
                                    {
                                        Console.WriteLine(detail.UserCollectionDetailsID(id));
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(detail.UserCollectionDetailsID(id));
                                    }
                                    catch (FormatException fe)
                                    {
                                        Console.WriteLine("Invalid User Collection ID.");
                                    }
                                    catch (KeyNotFoundException knfe)
                                    {
                                        Console.WriteLine("User Collection was not found.");
                                    }
                                }
                                break;
                            case "series":
                            case "ser":
                                if (string.IsNullOrEmpty(viewInput.Item2.Trim()))
                                {
                                    foreach (int id in database.Series.Select(x => x.Id))
                                    {
                                        Console.WriteLine(detail.BookSeriesDetailsID(id));
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        int id = int.Parse(viewInput.Item2);
                                        Console.Write(detail.BookSeriesDetailsID(id));
                                    }
                                    catch (FormatException fe)
                                    {
                                        Console.WriteLine("Invalid Book Series ID.");
                                    }
                                    catch (KeyNotFoundException knfe)
                                    {
                                        Console.WriteLine("Book Series was not found.");
                                    }
                                }
                                break;
                            case "selected":
                            case "s":
                                foreach (Book book in selectedBooks)
                                {
                                    Console.Write(detail.BookDetailsID(book.Id));
                                }
                                break;
                            default:
                                Console.WriteLine("View command was incorrect");
                                break;
                        }
                        break;

                    case "select":
                    case "s":
                        Tuple<string, string> selectInput = consoleInput.Item2.ToLower().Trim().SplitOnFirstBlank();
                        switch (selectInput.Item1)
                        {
                            case "book":
                            case "b":
                                ISet<int> bookIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach(int id in bookIds)
                                    selectedBooks.Add(database.Books.Find(id));

                                break;
                            case "series":
                            case "s":
                                ISet<int> seriesIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach (int id in seriesIds)
                                {
                                    foreach (Book book in database.Series.Include("Books").Where(x => x.Id == id).FirstOrDefault()?.Books)
                                        selectedBooks.Add(book);
                                }

                                break;
                            case "collection":
                            case "c":
                                ISet<int> collectionIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach (int id in collectionIds)
                                {
                                    foreach (Book book in database.UserCollections.Include("Books").Where(x => x.Id == id).FirstOrDefault()?.Books)
                                        selectedBooks.Add(book);
                                }

                                break;
                            case "all":
                            case "a":
                                selectedBooks.UnionWith(database.Books);
                                break;
                            case "clear":
                            case "clr":
                                selectedBooks.Clear();
                                break;
                            case "deselect":
                            case "d":
                                ISet<int> deselectIds = selectInput.Item2.GetIDsSeperatedBySpace();

                                foreach (int id in deselectIds)
                                    selectedBooks.Remove(database.Books.Find(id));
                                break;
                            default:
                                Console.WriteLine("Select command was incorrect");
                                break;
                        }
                        break;
                    case "export":
                    case "e":
                        ISet<EFile> exportFiles = new HashSet<EFile>();

                        Console.WriteLine("Selected books for export:");

                        foreach (Book book in selectedBooks)
                        {
                            database.Entry(book).Collection(f => f.Authors).Load();
                            database.Entry(book).Collection(f => f.Files).Load();
                            Console.WriteLine($"Id:{book.Id} {book.Name} by {(string.Join(", ", book.Authors.Select(x => x.Name)))}");
                            exportFiles.UnionWith(book.Files);
                        }

                        int exportChoice = 0;
                        string exportString = "";
                        do
                        {
                            Console.Write("Group exported books by series[0], author(1), both(2), neither(3): ");
                            exportString = Console.ReadLine().Trim();
                        } while (exportString != "" || !int.TryParse(exportString, out exportChoice) && (exportChoice == 0 || exportChoice == 1 || exportChoice == 2 || exportChoice == 3));

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
                            default:
                                break;
                        }

                        options.DestinationDirectory = consoleInput.Item2;
                        exporter.ExportBookFiles(exportFiles, options);

                        break;

                    /*

                    for (int i = 0; i < foundBooks.Count; i++)
                    {
                        Book book = new Book();
                        ParsedBook newBook = foundBooks[i];

                        Console.WriteLine($"{i+1}. {newBook.Title}");
                        Console.Write($"Title[{newBook.Title}]*: ");

                        book.name = GetNewOrDefaultInput(newBook.Title);

                        Console.Write($"Author[{newBook.Author}]*: ");
                        string authorName = GetNewOrDefaultInput(newBook.Author);

                        Author author = database.FindAuthor(authorName);
                        if (author == null)
                            author = database.AddAuthorDB(new Author() { name = authorName }) ;

                        Console.Write("Series: ");
                        string seriesName = Console.ReadLine().Trim();
                        if (seriesName != "")
                        {
                            int seriesNumber;
                            do
                            {
                                Console.Write("Series number: ");
                            } while (!Int32.TryParse(Console.ReadLine().Trim(), out seriesNumber));

                            book.seriesNumber = seriesNumber;
                        }

                        Console.WriteLine("Cover image: " + (newBook.Cover == null ? "[not found]" : "[found]") + " ");

                        book.isRead = ConsoleQuestion("Read", true);

                        Console.WriteLine($"Publisher: {newBook.Publisher}");

                        book = database.AddOrUpdateBook(book);

                        if (seriesName != "")
                        {
                            Series series = database.FindSeries(seriesName);

                            if (series == null)
                                series = new Series() { name = seriesName, author = author };

                            series.BookValues.Add(book);

                            database.AddOrUpdateSeries(series);
                            book.series = series;
                            database.AddOrUpdateBook(book);
                        }
                        book.FileValues.Add(database.AddFileDB(new Data.DomainModel.File() { book = book, fileBlob = System.IO.File.ReadAllBytes(fileList[i]), format = Path.GetExtension(fileList[i]) }));
                        database.AddOrUpdateBook(book);

                        database.AddBookAuthorLink(book, author);// TODO: Remove save changes
                    }

                    break;

                case "metadata":
                case "m":
                    try
                    {
                        //Console.WriteLine(database.GetBookMetadata(Int64.Parse(consoleInput.Item2)).GetJson());
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Invalid format");
                    }
                    break;

                case "find":
                case "f":
                    Tuple<string, string> findInput = consoleInput.Item2.ToLower().Trim().SplitOnFirstBlank();

                    string findType = findInput.Item1.ToLower();
                    string findWhat = findInput.Item2;
                    switch (findType)
                    {
                        case "book":
                        case "b":
                            IList<Book> books = database.FindBooks(findWhat);
                            foreach (Book book in books)
                            {
                                Console.WriteLine("Book: " + book.name);
                                Console.WriteLine("Authors:");
                                foreach (Author author in database.GetBookAuthors(book))
                                    Console.WriteLine("    " + author.name);
                                Console.WriteLine();
                            }
                            break;

                        case "author":
                        case "a":
                            IList<Author> authors = database.FindAuthors(findWhat);
                            foreach (Author author in authors)
                            {
                                Console.WriteLine(author);
                                Console.WriteLine("Books: ");
                                foreach (Book book in database.GetAuthorBooks(author))
                                    Console.WriteLine("    " + book.name);
                                Console.WriteLine();
                            }
                            break;

                        default:
                            Console.WriteLine("Find command was incorrect");
                            break;
                    }
                    break;

                case "view":
                case "v":
                    Tuple<string, string> viewInput = consoleInput.Item2.ToLower().Trim().SplitOnFirstBlank();
                    switch(viewInput.Item1)
                    {
                        case "details":
                        case "d":
                            Console.Write(database.GetBookFromID(Int64.Parse(viewInput.Item2)).GetDetails()); // TODO: Error handling
                            break;

                        case "all":
                        case "a":
                            foreach (Book book in database.Books)
                                Console.WriteLine(book);
                            break;

                        case "author":
                        case "au":
                            foreach (Author author in database.Authors)
                                Console.WriteLine(author);
                            break;

                        case "collection":
                        case "c":
                            if(viewInput.Item2 == "")
                                foreach (Collection collection in database.Collections)
                                    Console.WriteLine(collection);
                            else
                                try
                                {
                                    Collection collection = database.GetCollectionFromID(Int64.Parse(viewInput.Item2));
                                    if(collection != null)
                                        Console.WriteLine(collection);
                                    else
                                        Console.WriteLine("Collection does not exit");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Invalid collection id");
                                }
                            break;

                        case "series":
                        case "s":
                            if (viewInput.Item2 == "")
                                foreach (Series series in database.Series)
                                    Console.WriteLine(series.name);
                            else
                                try
                                {
                                    Series series = database.GetSeriesFromID(Int64.Parse(viewInput.Item2));
                                    if (series != null)
                                        Console.WriteLine(series);
                                    else
                                        Console.WriteLine("Series does not exit");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Invalid series id");
                                }
                            break;

                        default:
                            Console.WriteLine("View command was incorrect");
                            break;
                    }
                    */

                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            } while (command != "exit");

            Console.WriteLine("Exiting...");
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}