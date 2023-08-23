using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

/// <summary>
/// User provided data to describe crawling.
/// </summary>
[Table("WebsiteRecord")]
public class WebsiteRecordData
{
    [Key]
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<WebTag> Tags { get; set; } = new();
    public DateTime Created { get; set; } = DateTime.Now;
    public CrawlInfoData CrawlInfoData { get; set; } = null!;
}
