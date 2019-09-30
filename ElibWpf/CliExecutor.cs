using ElibWpf.Database;
using ElibWpf.DomainModel;
using ElibWpf.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf
{
    class CliExecutor
    {
        private DatabaseContext database;
        public CliExecutor()
        {
            database = DatabaseContext.GetInstance();
            Console.WriteLine("Starting eLIB in CLI mode.\nWELCOME TO ELIB COMMAND LINE.\n");
        }

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
                    case "listall":
                        database.ListAllBooks();
                        break;
                    case "listallauthors":
                        database.ListAllAuthors();
                        break;
                    case "import":
                        database.ImportBook(consoleInput.Item2);
                        break;
                    case "metadata":
                        database.BookMetadata(Int64.Parse(consoleInput.Item2)); // TODO: Error handling
                        break;
                    case "find":
                        Tuple<string, string> findInput = consoleInput.Item2.ToLower().Trim().SplitOnFirstBlank();

                        string findType = findInput.Item1.ToLower();
                        string findWhat = findInput.Item2;
                        switch (findType)
                        {
                            case "book":
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

                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }

            } while (command != "exit");

            Console.WriteLine("Exiting...");
        }
    }
}
