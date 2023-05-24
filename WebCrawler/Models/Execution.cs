using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

[Table("Execution")]
public class Execution
{
    [Key]
    public int Id { get; set; }
    
    public ICollection<Website> Websites { get; set; } = new List<Website>();
    
    public DateTime Started { get; set; } = DateTime.MinValue;
    public DateTime Finished { get; set; } = DateTime.MinValue;

    [ForeignKey("WebsiteRecord")]
    public int WebsiteRecordId { get; set; }
    public WebsiteRecord WebsiteRecord { get; set; } = new();
}
