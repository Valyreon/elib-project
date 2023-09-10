namespace Valyreon.Elib.BookDataAPI.GoogleBooks.Dtos;

public class VolumeSearchResults
{
    public string Kind { get; set; }
    public int? TotalItems { get; set; }
    public List<VolumeResultItem>? Items { get; set; }
}
