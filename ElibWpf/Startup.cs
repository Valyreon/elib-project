using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElibWpf.Database;
using ElibWpf.DomainModel;
using ElibWpf.Helpers;

namespace ElibWpf
{
    public class Startup : System.Windows.Application
    {
        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            Console.WriteLine("Initializing local DB");
            DatabaseContext database = new DatabaseContext();
            Console.WriteLine("Checking arguments");
            // Check if app is being run in CLI mode
            string[] args = Environment.GetCommandLineArgs();
            foreach (string x in args)
            {
                Console.WriteLine(x);
            }
            if (args.Contains("-cli")) // Don't show the GUI
            {
                Console.WriteLine("Starting eLIB in CLI mode.\nWELCOME TO ELIB COMMAND LINE.\n");
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
                            switch(findType)
                            {
                                case "book":
                                    Book[] books = database.FindBooks(findWhat);
                                    foreach (Book book in books)
                                    {
                                        Console.WriteLine("Book: " + book.name);
                                        Console.WriteLine("Authors:");
                                        foreach (Author author in database.GetBookAuthors(book))
                                            Console.WriteLine("    " + author.name);
                                        Console.WriteLine();
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
            else
            {
                ElibWpf.App app = new ElibWpf.App();
                app.InitializeComponent();
                app.Run();
            }
        }

    }
}
