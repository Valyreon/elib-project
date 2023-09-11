namespace Valyreon.Elib.BookDataAPI.GoogleBooks.Dtos;

public class SaleInfo
{
    public string Country { get; set; }
    public bool? IsEbook { get; set; }
    public string Saleability { get; set; }
}

public class VolumeResultItem
{
    public AccessInfo? AccessInfo { get; set; }
    public string Etag { get; set; }
    public string Id { get; set; }
    public string Kind { get; set; }
    public SaleInfo? SaleInfo { get; set; }
    public SearchInfo? SearchInfo { get; set; }
    public string SelfLink { get; set; }
    public VolumeInfo? VolumeInfo { get; set; }
}

public class SearchInfo
{
    public string TextSnippet { get; set; }
}
