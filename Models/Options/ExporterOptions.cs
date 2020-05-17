namespace Models.Options
{
    public class ExporterOptions
    {
        public string DestinationDirectory { get; set; }
        public bool GroupByAuthor { get; set; }
        public bool GroupBySeries { get; set; }
    }
}