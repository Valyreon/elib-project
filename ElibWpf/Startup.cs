using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElibWpf.Database;

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
            //check if app is being run in CLI mode
            string[] args = Environment.GetCommandLineArgs();
            foreach (var x in args)
            {
                Console.WriteLine(x);
            }
            if (args.Contains("-cli"))
            {
                Console.WriteLine("Starting eLIB in CLI mode.");
                // don't show the UI
                String consoleInput;
                Console.WriteLine("\nWELCOME TO ELIB COMMAND LINE.\n");
                do
                {
                    Console.Write(">> ");
                    consoleInput = Console.ReadLine();
                    consoleInput = consoleInput.Trim();
                    string[] consoleInputArray = consoleInput.Split(' ');
                    if (consoleInputArray.Length > 0)
                    {
                        switch (consoleInputArray[0].ToLower())
                        {
                            case "add":
                                database.AddBook(consoleInput.Substring(3, consoleInput.Length - 3));
                                break;
                            case "listall":
                                database.ListAllBooks();
                                break;
                            default:
                                Console.WriteLine("Unknown command");
                                break;
                        }
                    }

                } while (consoleInput.ToLower() != "exit");
                if (consoleInput.ToLower() == "exit")
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
