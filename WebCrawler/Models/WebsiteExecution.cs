using System.ComponentModel.DataAnnotations;

namespace WebCrawler.Models;

/// <summary>
/// Represents one execution of the executor.
/// </summary>
public class WebsiteExecution
{
    [Key]
    public int Id { get; set; }
    public string AdjacencyListJson { get; set; } = string.Empty;

    /// <summary>
    /// TODO: Before storing to database, convert to adjacency list 
    /// </summary>
    public WebsiteGraph WebsiteGraph { get; set; }

    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }

    /// <summary>
    /// Don't use! Needs SQLlite.
    /// </summary>
    public WebsiteExecution()
    {
    }
    
    public WebsiteExecution(WebsiteGraph websiteGraph)
    {
        WebsiteGraph = websiteGraph;
    }
}

