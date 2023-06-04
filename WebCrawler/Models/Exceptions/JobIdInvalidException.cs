namespace WebCrawler.Models.Exceptions;

public class JobIdInvalidException : ArgumentException
{
    public JobIdInvalidException(string message) : base(message) { }
}
