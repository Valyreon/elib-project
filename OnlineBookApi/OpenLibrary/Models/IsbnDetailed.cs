using System.Collections.Generic;

namespace OnlineBookApi.OpenLibrary.Models
{
    public class IsbnDetailed
    {
        public List<Author> authors { get; set; }
        public string by_statement { get; set; }
        public Classifications classifications { get; set; }
        public Identifiers identifiers { get; set; }
        public string key { get; set; }
        public string notes { get; set; }
        public int number_of_pages { get; set; }
        public string pagination { get; set; }
        public string publish_date { get; set; }
        public List<PublishPlace> publish_places { get; set; }
        public List<Publisher> publishers { get; set; }
        public List<SubjectPlace> subject_places { get; set; }
        public List<Subject> subjects { get; set; }
        public string subtitle { get; set; }
        public List<TableOfContent> table_of_contents { get; set; }
        public string title { get; set; }
        public string url { get; set; }

        public class Publisher
        {
            public string name { get; set; }
        }

        public class Identifiers
        {
            public List<string> isbn_10 { get; set; }
            public List<string> isbn_13 { get; set; }
            public List<string> openlibrary { get; set; }
        }

        public class TableOfContent
        {
            public string label { get; set; }
            public int level { get; set; }
            public string pagenum { get; set; }
            public string title { get; set; }
        }

        public class Classifications
        {
            public List<string> dewey_decimal_class { get; set; }
        }

        public class SubjectPlace
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        public class Author
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        public class Subject
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        public class PublishPlace
        {
            public string name { get; set; }
        }
    }
}