using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

public class Execution
{
    public WebsiteRecord EntryWebsite { get; set; } = new();
    
    public ICollection<Website> Websites { get; set; } = new List<Website>();
    
    public DateTime Started { get; set; } = DateTime.MinValue;
    public DateTime Finished { get; set; } = DateTime.MinValue;

    [ForeignKey("WebsiteRecord")]
    public int WebsiteRecordId { get; set; }
    public WebsiteRecord WebsiteRecord { get; set; } = new();
}
