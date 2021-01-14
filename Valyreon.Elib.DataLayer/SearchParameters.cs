namespace Valyreon.Elib.DataLayer
{
    public class SearchParameters
    {
        public bool SearchByAuthor { get; set; }
        public bool SearchByTitle { get; set; } = true;
        public bool SearchBySeries { get; set; }
        public string Token { get; set; } = "";

        public SearchParameters Clone()
        {
            return new SearchParameters
            {
                SearchByAuthor = SearchByAuthor,
                SearchBySeries = SearchBySeries,
                SearchByTitle = SearchByTitle,
                Token = Token
            };
        }

        public override bool Equals(object obj)
        {
            return obj is SearchParameters parameters &&
                   SearchByAuthor == parameters.SearchByAuthor &&
                   SearchByTitle == parameters.SearchByTitle &&
                   SearchBySeries == parameters.SearchBySeries &&
                   Token == parameters.Token;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(SearchByAuthor, SearchByTitle, SearchBySeries, Token);
        }
    }
}
