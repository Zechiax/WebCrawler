namespace WebCrawler.Models;

/// <summary>
/// Represents the crawled website graph.
/// </summary>
public class WebsiteGraph
{
    /// <summary>
    /// Website where the crawling starts.
    /// </summary>
    public Website EntryWebsite { get; init; }

    public WebsiteGraph(Website entryWebsite)
    {
        EntryWebsite = entryWebsite;
    }

    /// <summary>
    /// Returns adjacency list of this graph.
    /// </summary>
    /// <returns></returns>
    public AdjacencyList GetAdjacencyListGraphRepresentation()
    {
        Dictionary<Website, List<Website>> adjacencyListData = new();
        AdjacencyListTraversal(EntryWebsite, adjacencyListData);
        return new AdjacencyList(adjacencyListData, EntryWebsite);
    }

    private void AdjacencyListTraversal(Website website, Dictionary<Website, List<Website>> adjacencyListData)
    {
        if (adjacencyListData.ContainsKey(website))
        {
            return;
        }

        adjacencyListData[website] = new List<Website>();

        foreach(Website outgoingWebsite in website.OutgoingWebsites)
        {
            adjacencyListData[website].Add(outgoingWebsite);
            AdjacencyListTraversal(outgoingWebsite, adjacencyListData);
        }
    }
}

