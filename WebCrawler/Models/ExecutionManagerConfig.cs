namespace WebCrawler.Models;

public record class ExecutionManagerConfig
{
    public int CrawlersCount { get; set; }
    public Type TWebsiteProvider { get; set; } = typeof(WebsiteProvider); 
}
