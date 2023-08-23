namespace WebCrawler.Models;

public class WebsiteRecord
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<WebTag> Tags { get; set; } = new();
    public DateTime Created { get; set; } = DateTime.Now;
    public CrawlInfo CrawlInfo { get; set; } = null!;
}