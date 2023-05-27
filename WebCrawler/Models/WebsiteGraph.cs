namespace WebCrawler.Models;

public class WebsiteGraph
{
    public Website EntryWebsite { get; init; }

    public WebsiteGraph(Website entryWebsite)
    {
        EntryWebsite = entryWebsite;
    }

    public NeighboursList GetNeighboursListGraphRepresentation()
    {
        Dictionary<Website, List<Website>> neighboursListData = new();
        NeighboursListTraversal(EntryWebsite, neighboursListData);
        return new NeighboursList(neighboursListData);
    }

    private void NeighboursListTraversal(Website website, Dictionary<Website, List<Website>> neighboursListData)
    {
        if (neighboursListData.ContainsKey(website))
        {
            return;
        }

        neighboursListData[website] = new List<Website>();

        foreach(Website outgoingWebsite in website.OutgoingWebsites)
        {
            neighboursListData[website].Add(outgoingWebsite);
            NeighboursListTraversal(outgoingWebsite, neighboursListData);
        }
    }
}

