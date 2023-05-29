namespace WebCrawler.Models;

public class Website
{
    public string Url { get; set; }
    public TimeSpan CrawlTime { get; set; } = TimeSpan.Zero;
    public string Title { get; set; } = string.Empty;
    public List<Website> OutgoingWebsites { get; set; } = new();

    public Website(string url)
    {
        Url = url;
    }

    public override int GetHashCode()
    {
        return Url.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Title}:{Url}"; 
    }
}
