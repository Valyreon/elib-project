﻿using System.Collections.Generic;

namespace OnlineBookApi.OpenLibrary.Models
{
    public class SearchQuery
    {
        public List<Doc> docs { get; set; }
        public int num_found { get; set; }
        public int numFound { get; set; }
        public int start { get; set; }

        public class Doc
        {
            public List<string> author_key { get; set; }
            public List<string> author_name { get; set; }
            public List<string> contributor { get; set; }
            public string cover_edition_key { get; set; }
            public int? cover_i { get; set; }
            public int ebook_count_i { get; set; }
            public int edition_count { get; set; }
            public List<string> edition_key { get; set; }
            public int first_publish_year { get; set; }
            public bool has_fulltext { get; set; }
            public List<string> isbn { get; set; }
            public string key { get; set; }
            public List<string> language { get; set; }
            public int last_modified_i { get; set; }
            public List<string> place { get; set; }
            public List<string> publish_date { get; set; }
            public List<string> publish_place { get; set; }
            public List<int> publish_year { get; set; }
            public List<string> publisher { get; set; }
            public List<string> seed { get; set; }
            public List<string> subject { get; set; }
            public List<string> text { get; set; }
            public string title { get; set; }
            public string title_suggest { get; set; }
            public string type { get; set; }
        }
    }
}