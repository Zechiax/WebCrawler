namespace WebCrawler.Models;

public class Website
{
    public string Url { get; set; }
    public TimeSpan CrawlTime { get; set; }
    public string Title { get; set; }
    public List<Website> OutgoingLinks { get; set; } = new();
}
