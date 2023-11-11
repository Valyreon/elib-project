namespace Valyreon.Elib.BookDataAPI.GoogleBooks.Dtos;

public class AccessInfo
{
    public string AccessViewStatus { get; set; }
    public string Country { get; set; }
    public bool? Embeddable { get; set; }
    public FormatAvailability Epub { get; set; }
    public FormatAvailability Pdf { get; set; }
    public bool? PublicDomain { get; set; }
    public bool? QuoteSharingAllowed { get; set; }
    public string TextToSpeechPermission { get; set; }
    public string Viewability { get; set; }
    public string WebReaderLink { get; set; }
}

public class FormatAvailability
{
    public bool? IsAvailable { get; set; }
}
