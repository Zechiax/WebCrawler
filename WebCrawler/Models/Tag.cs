using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

[Table("Tag")]
public class Tag
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}