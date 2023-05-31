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
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }

    public WebsiteExecution()
    {
    }
    
    public WebsiteExecution(WebsiteGraph websiteGraph)
    {
        SetWebsiteGraph(websiteGraph);
    }
    
    public void SetWebsiteGraph(WebsiteGraph websiteGraph)
    {
        AdjacencyListJson = AdjacencyList.JsonConverter.Serialize(websiteGraph.GetAdjacencyListGraphRepresentation());
    }

    public AdjacencyList GetAdjacencyList()
    {
        return AdjacencyList.JsonConverter.Deserialize(AdjacencyListJson);
    }
}

