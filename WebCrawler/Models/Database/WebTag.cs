using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;

[Table("Tag")]
public class WebTag
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Constructor for EF.
    /// </summary>
    public WebTag()
    {

    }

    public WebTag(string name)
    {
        Name = name;
    }
}