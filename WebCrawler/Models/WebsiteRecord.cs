using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

[Table("WebsiteRecord")]
public class WebsiteRecord
{
    [Key]
    public int Id { get; set; }

    public string Url { get; set; } = string.Empty;
    public string Regex { get; set; } = string.Empty;
    public TimeSpan Periodicity { get; set; } = TimeSpan.Zero;
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    
    public DateTime Created { get; set; } = DateTime.MinValue;
    public ExecutorData? LastExecution { get; set; }
}
