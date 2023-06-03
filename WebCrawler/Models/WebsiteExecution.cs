namespace WebCrawler.Models;

public class WebsiteExecution
{
    public CrawlInfo Info { get; init; }
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }

    public WebsiteExecution(CrawlInfo info)
    {
        Info = info;
    }
}
