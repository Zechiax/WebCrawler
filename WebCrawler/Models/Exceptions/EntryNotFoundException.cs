namespace WebCrawler.Models.Exceptions;

public class EntryNotFoundException : DatabaseException
{
    public EntryNotFoundException(string message) : base(message)
    {
    }
}