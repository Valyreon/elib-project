namespace Valyreon.Elib.BookDataAPI;

public interface IBookInformationAPI
{
    public Task<BookInformation> GetByIsbnAsync(string isbn, bool includeCover = true);
    //public BookInformation SearchAsync(string isbn, bool includeCover = true);
}
