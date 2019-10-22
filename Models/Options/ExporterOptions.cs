namespace Models.Options
{
    public class ExporterOptions
    {
        public bool GroupByAuthor { get; set; }
        public bool GroupBySeries { get; set; }
        public bool CreateNewDirectory { get; set; }
        public string NewDirectoryName { get; set; }
        public string DestinationDirectory { get; set; }
    }
}