using Newtonsoft.Json;
using WebCrawler.Models;

namespace WebCrawler.Models;

public readonly partial struct WebsiteGraphSnapshot
{
    public static class JsonConverterToDomainView
    {
        private class WebsiteNodeWithNeighbours
        {
            public string Url { get; set; } = null!;
            public string Title { get; set; } = null!;
            public TimeSpan CrawlTime { get; set; }
            public IEnumerable<string> Neighbours { get; set; } = null!;
        }

        public static string Serialize(WebsiteGraphSnapshot graph)
        {
            List<WebsiteNodeWithNeighbours> domainNodesWithNeighbours = new();

            HashSet<string> alreadyAddedDomains = new();
            foreach(KeyValuePair<Website, List<Website>> websiteNeighbours in graph.Data)
            {
                string domain;
                try
                {
                    domain = new Uri(websiteNeighbours.Key.Url).Host;
                }
                catch
                {
                    continue;
                }

                if (!alreadyAddedDomains.Contains(domain))
                {
                    TimeSpan crawlTimeSum = TimeSpan.Zero;

                    alreadyAddedDomains.Add(domain);
                    var domainNodes = graph.Data.Where(nodeAndNeighbours =>
                    {
                        try
                        {
                            return new Uri(nodeAndNeighbours.Key.Url).Host == domain;
                        }
                        catch { return false; }
                    });

                    Dictionary<string, Website> allNeighboursOfDomainNodes = new();
                    foreach (var domainNode in domainNodes)
                    {
                        crawlTimeSum += domainNode.Key.CrawlTime;
                        foreach (var neighbour in domainNode.Value)
                        {
                            allNeighboursOfDomainNodes[neighbour.Url] = neighbour;
                        }
                    }

                    domainNodesWithNeighbours.Add(new WebsiteNodeWithNeighbours
                    {
                        Url = domain,
                        Title = domain,
                        CrawlTime = crawlTimeSum,
                        Neighbours = allNeighboursOfDomainNodes.Values.Select(node =>
                        {
                            return new Uri(node.Url).Host;
                        })
                    });
                }
            }

            // We add the entry website to the graph, so that we can easily deserialize it later.
            return JsonConvert.SerializeObject(new
            {
                EntryUrl = graph.EntryWebsite?.Url,
                Graph = domainNodesWithNeighbours
            });
        }

        /// <summary>
        /// Not implemented, don't care for now.
        /// </summary>
        /// <param name="json"></param>
        public static WebsiteGraphSnapshot Deserialize(string json)
        {
            throw new NotImplementedException();
        }
    }
}