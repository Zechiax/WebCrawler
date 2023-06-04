using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models;

public class WebsiteExecution
{
    [Key]
    public int Id { get; set; }
    public CrawlInfo Info { get; init; } = null!;
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }

    /// <summary>
    /// Constructor for EF Core
    /// </summary>
    public WebsiteExecution()
    {}

    public WebsiteExecution(CrawlInfo info)
    {
        Info = info;
    }
}
