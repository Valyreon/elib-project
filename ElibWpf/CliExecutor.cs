﻿using ElibWpf.Database;
using ElibWpf.DomainModel;
using ElibWpf.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf

{
    /// <summary>  
    ///  This class is used for CLI functionality.  
    /// </summary>  
    class CliExecutor
    {
        private DatabaseContext database;
        public CliExecutor()
        {
            database = DatabaseContext.GetInstance();
            Console.WriteLine("Starting eLIB in CLI mode.\nWELCOME TO ELIB COMMAND LINE.\n");
        }

        /// <summary>  
        ///  Starts the CLI loop until the keyword 'exit' is inputted.  
        /// </summary>  
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
                    case "listall":
                        foreach(Book book in database.Books)
                            Console.WriteLine(book);
                        break;
                    case "listallauthors":
                        foreach (Author author in database.Authors)
                            Console.WriteLine(author);
                        break;
                    case "import":
                        database.ImportBook(consoleInput.Item2);
                        break;
                    case "metadata":
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
