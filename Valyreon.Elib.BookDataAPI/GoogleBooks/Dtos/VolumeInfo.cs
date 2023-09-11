namespace Valyreon.Elib.BookDataAPI.GoogleBooks.Dtos;

public class ImageLinks
{
    public string SmallThumbnail { get; set; }
    public string Thumbnail { get; set; }
}

public class PanelizationSummary
{
    public bool? ContainsEpubBubbles { get; set; }
    public bool? ContainsImageBubbles { get; set; }
}

public class ReadingModes
{
    public bool? Image { get; set; }
    public bool? Text { get; set; }
}

public class VolumeInfo
{
    public bool? AllowAnonLogging { get; set; }
    public List<string> Authors { get; set; }
    public double? AverageRating { get; set; }
    public string CanonicalVolumeLink { get; set; }
    public List<string> Categories { get; set; }
    public string ContentVersion { get; set; }
    public string Description { get; set; }
    public ImageLinks? ImageLinks { get; set; }
    public List<IndustryIdentifier>? IndustryIdentifiers { get; set; }
    public string InfoLink { get; set; }
    public string Language { get; set; }
    public string MaturityRating { get; set; }
    public int? PageCount { get; set; }
    public PanelizationSummary? PanelizationSummary { get; set; }
    public string PreviewLink { get; set; }
    public string PrintType { get; set; }
    public string PublishedDate { get; set; }
    public string Publisher { get; set; }
    public int? RatingsCount { get; set; }
    public ReadingModes? ReadingModes { get; set; }
    public string Title { get; set; }
}

public class IndustryIdentifier
{
    public string Identifier { get; set; }
    public string Type { get; set; }
}
