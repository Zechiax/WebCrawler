using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models.Database;

public class WebsiteExecutionData
{
    [Key]
    public int Id { get; set; }
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }
    public string? WebsiteGraphSnapshotJson { get; set; }
}