using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

[Table("Website")]
public class Website
{
    [Key]
    public int Id { get; set; }
    
    public string Url { get; set; } = string.Empty;
    public TimeSpan CrawlTime { get; set; } = TimeSpan.Zero;
    public string Title { get; set; } = string.Empty;
    
    public ICollection<Website> OutgoingLinks { get; set; } = new List<Website>();
    
    [ForeignKey("WebsiteRecord")]
    public int WebsiteRecordId { get; set; }
    public WebsiteRecord WebsiteRecord { get; set; } = new();
}
