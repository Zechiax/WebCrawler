using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models;

public class WebsiteExecution
{
    [Key]
    public int Id { get; set; }
    public WebsiteGraph WebsiteGraph { get; set; }
    public DateTime? Started { get; set; } = null;
    public DateTime? Finished { get; set; } = null;

    public WebsiteExecution(WebsiteGraph websiteGraph)
    {
        WebsiteGraph = websiteGraph;
    }
}

