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
    }
}