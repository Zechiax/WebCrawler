using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace WebCrawler.Models;

[Table("Execution")]
public class Execution
{
    [Key]
    public int Id { get; set; }
    
    public string? WebTreeJson { get; set; } = string.Empty;
    
    public DateTime Started { get; set; } = DateTime.MinValue;
    public DateTime Finished { get; set; } = DateTime.MinValue;

    [ForeignKey("WebsiteRecord")]
    public int WebsiteRecordId { get; set; }
    public WebsiteRecord WebsiteRecord { get; set; } = new();
    
    public Website? GetWebTree()
    {
        if (string.IsNullOrEmpty(WebTreeJson))
        {
            return null;
        }
        
        return JsonSerializer.Deserialize<Website>(WebTreeJson);
    }
    
    public void SetWebTree(Website website)
    {
        WebTreeJson = JsonSerializer.Serialize(website);
    }
}
