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
                SearchByAuthor = this.SearchByAuthor,
                SearchBySeries = this.SearchBySeries,
                SearchByTitle = this.SearchByTitle,
                Token = this.Token
            };
        }

        public override bool Equals(object obj)
        {
            return obj is SearchParameters parameters &&
                   this.SearchByAuthor == parameters.SearchByAuthor &&
                   this.SearchByTitle == parameters.SearchByTitle &&
                   this.SearchBySeries == parameters.SearchBySeries &&
                   this.Token == parameters.Token;
        }

        public override int GetHashCode()
        {
            int hashCode = 63130288;
            hashCode = hashCode * -1521134295 + this.SearchByAuthor.GetHashCode();
            hashCode = hashCode * -1521134295 + this.SearchByTitle.GetHashCode();
            hashCode = hashCode * -1521134295 + this.SearchBySeries.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Token);
            return hashCode;
        }
    }
}