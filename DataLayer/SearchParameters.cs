using System.Collections.Generic;

namespace DataLayer
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
            var hashCode = 63130288;
            hashCode = (hashCode * -1521134295) + SearchByAuthor.GetHashCode();
            hashCode = (hashCode * -1521134295) + SearchByTitle.GetHashCode();
            hashCode = (hashCode * -1521134295) + SearchBySeries.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Token);
            return hashCode;
        }
    }
}
