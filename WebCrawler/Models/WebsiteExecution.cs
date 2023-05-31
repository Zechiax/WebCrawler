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

    /// TODO: don't save to database directly, just serialize it to the <see cref="AdjacencyListJson"/> at the moment when saving to database
    public WebsiteGraph WebsiteGraph { get; set; } = null!;

    public DateTime? Started { get; set; } = null;
    public DateTime? Finished { get; set; } = null;
    public WebsiteRecord WebsiteRecord { get; set; } = null!;
    public int WebsiteRecordId { get; set; }

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

