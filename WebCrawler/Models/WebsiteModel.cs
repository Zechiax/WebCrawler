namespace WebCrawler.Models;

public record class WebsiteModel
{
    public string Url { get; set; }
    public TimeSpan CrawlTime { get; set; }
    public string Title { get; set; }
    public List<WebsiteModel> OutgoingLinks { get; set; } = new();
}
