using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Models.Helpers;
using EbookTools;
using EbookTools.Mobi;
using EbookTools.Epub;
using Data.DomainModel;
using Data;

namespace Cli

{
    /// <summary>  
    ///  This class is used for CLI functionality.  
    /// </summary>  
    public class CliExecutor
    {
        private DatabaseContext database;
        public CliExecutor()
        {
            database = DatabaseContext.GetInstance();
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

            return consoleKey == ConsoleKey.Y  ||  consoleKey == ConsoleKey.Enter && !inverted;
        }
        private string GetNewOrDefaultInput(string def)
        {
            string newString = Console.ReadLine().Trim();

            if (newString == "")
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
                        // TODO: Check if book exists
                        Regex splitRegex = new Regex("(?<=\")[^\"]*(?=\")|[^\" ]+");
                        List<string> fileList = splitRegex.Matches(consoleInput.Item2).Cast<Match>().Select(x => x.Value).ToList();

                        List<ParsedBook> foundBooks = new List<ParsedBook>();
                        foreach(string filePath in fileList)
                        {
                            if(System.IO.File.Exists(filePath))
                            {
                                StreamReader bookStream = new StreamReader(filePath);
                                switch (Path.GetExtension(filePath))
                                {
                                    case ".mobi":
                                        foundBooks.Add(new MobiParser(bookStream.BaseStream).Parse());
                                        break;
                                    case ".epub":
                                        foundBooks.Add(new EpubParser(bookStream.BaseStream).Parse());
                                        break;
                                    default:
                                        Console.WriteLine($"File {filePath} has an unsupported extension.");
                                        break;
                                }
                            }
                            else
                                Console.WriteLine($"File {filePath} does not exist.");
                        }

                        if (foundBooks.Count == 0)
                        {
                            Console.WriteLine("No books were found.");
                            break;
                        }

                        Console.WriteLine("Found books:");
                        for (int i = 1; i <= foundBooks.Count; i++)
                            Console.WriteLine($"{i}. {foundBooks[i - 1].Title}");

                        if (!ConsoleQuestion("Do you want to continue?", false))
                            break;

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
                            Console.WriteLine(database.GetBookMetadata(Int64.Parse(consoleInput.Item2)).GetJson());
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
                                    /*Console.WriteLine("Books: ");
                                    foreach (Book book in database.GetAuthorBooks(author))
                                        Console.WriteLine("    " + book.name);
                                    Console.WriteLine();*/
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

                        break;
                    

                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }

            } while (command != "exit");

            Console.WriteLine("Exiting...");
        }
    }
}
