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

    // REMARK: Don't use. Needs SQLlite.
    public WebsiteExecution()
    {
    }
    
    public WebsiteExecution(WebsiteGraph websiteGraph)
    {
        WebsiteGraph = websiteGraph;
        SetWebsiteGraph(websiteGraph);
    }
    
    public void SetWebsiteGraph(WebsiteGraph websiteGraph)
    {
        AdjacencyListJson = WebsiteGraphSnapshot.JsonConverter.Serialize(websiteGraph.GetSnapshot());
    }

    public WebsiteGraphSnapshot GetAdjacencyList()
    {
        return WebsiteGraphSnapshot.JsonConverter.Deserialize(AdjacencyListJson);
    }
}

