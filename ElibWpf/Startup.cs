using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElibWpf.Database;
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
