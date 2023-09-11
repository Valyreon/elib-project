namespace Valyreon.Elib.BookDataAPI.GoogleBooks.Dtos;

public class VolumeSearchResults
{
    public List<VolumeResultItem>? Items { get; set; }
    public string Kind { get; set; }
    public int? TotalItems { get; set; }
}
