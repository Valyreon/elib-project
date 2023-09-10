namespace Valyreon.Elib.BookDataAPI.GoogleBooks.Dtos;

public class VolumeInfo
{
    public string Title { get; set; }
    public List<string> Authors { get; set; }
    public string Publisher { get; set; }
    public string PublishedDate { get; set; }
    public string Description { get; set; }
    public List<IndustryIdentifier>? IndustryIdentifiers { get; set; }
    public ReadingModes? ReadingModes { get; set; }
    public int? PageCount { get; set; }
    public string PrintType { get; set; }
    public List<string> Categories { get; set; }
    public double? AverageRating { get; set; }
    public int? RatingsCount { get; set; }
    public string MaturityRating { get; set; }
    public bool? AllowAnonLogging { get; set; }
    public string ContentVersion { get; set; }
    public PanelizationSummary? PanelizationSummary { get; set; }
    public ImageLinks? ImageLinks { get; set; }
    public string Language { get; set; }
    public string PreviewLink { get; set; }
    public string InfoLink { get; set; }
    public string CanonicalVolumeLink { get; set; }
}

public class PanelizationSummary
{
    public bool? ContainsEpubBubbles { get; set; }
    public bool? ContainsImageBubbles { get; set; }
}

public class ImageLinks
{
    public string SmallThumbnail { get; set; }
    public string Thumbnail { get; set; }
}

public class ReadingModes
{
    public bool? Text { get; set; }
    public bool? Image { get; set; }
}

public class IndustryIdentifier
{
    public string Type { get; set; }
    public string Identifier { get; set; }
}
