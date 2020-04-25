namespace Models.Options
{
    public class ExporterOptions
    {
        public bool GroupByAuthor { get; set; }
        public bool GroupBySeries { get; set; }
        public string DestinationDirectory { get; set; }
    }
}