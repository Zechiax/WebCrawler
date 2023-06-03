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
    public WebsiteGraphSnapshot GetSnapshot()
    {
        Dictionary<Website, List<Website>> adjacencyListData = new();
        GraphTraversal(EntryWebsite, adjacencyListData);
        return new WebsiteGraphSnapshot(adjacencyListData, EntryWebsite);
    }

    private void GraphTraversal(Website website, Dictionary<Website, List<Website>> adjacencyListData)
    {
        if (adjacencyListData.ContainsKey(website))
        {
            return;
        }

        adjacencyListData[website] = new List<Website>();

        lock(website)
        {
            foreach(Website outgoingWebsite in website.OutgoingWebsites)
            {
                adjacencyListData[website].Add(outgoingWebsite);
                GraphTraversal(outgoingWebsite, adjacencyListData);
            }
        }
    }
}

