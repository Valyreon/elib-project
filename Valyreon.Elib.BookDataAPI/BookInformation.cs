namespace Valyreon.Elib.BookDataAPI;

public record BookInformation(
    string Title,
    string Description,
    IReadOnlyList<string> Authors,
    IReadOnlyList<string> Subjects,
    string Publisher,
    string PublishedDate,
    string ISBN,
    int? PageCount,
    byte[] Cover);
