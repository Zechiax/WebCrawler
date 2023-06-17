using Newtonsoft.Json;

namespace WebCrawler.Models;

public readonly partial struct WebsiteGraphSnapshot
{
    public static class JsonConverter
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
            IEnumerable<WebsiteNodeWithNeighbours> websiteVertices = graph.Data.Select(
                websiteNeighbours => new WebsiteNodeWithNeighbours
                {
                    Url = websiteNeighbours.Key.Url,
                    Title = websiteNeighbours.Key.Title,
                    CrawlTime = websiteNeighbours.Key.CrawlTime,
                    Neighbours = websiteNeighbours.Value.Select(neighbour => neighbour.Url)
                }
            );
            
            // We add the entry website to the graph, so that we can easily deserialize it later.
            return JsonConvert.SerializeObject(new
            {
                EntryUrl = graph.EntryWebsite?.Url,
                Graph = websiteVertices
            });
        }

        public static WebsiteGraphSnapshot Deserialize(string json)
        {
            var snapshot = JsonConvert.DeserializeAnonymousType(json, new
            {
                EntryUrl = string.Empty,
                Graph = Enumerable.Empty<WebsiteNodeWithNeighbours>()
            });

            if(snapshot is null)
            {
                throw new ArgumentException("Can't deserialize from given json.");
            }

            Dictionary<string, Website> urlToWebsiteLookup = snapshot.Graph
                .ToDictionary(model => model.Url, model => new Website(model.Url) { Title = model.Title, CrawlTime = model.CrawlTime });

            Dictionary<Website, List<Website>> data = urlToWebsiteLookup.Values.ToDictionary(website => website, _ => new List<Website>()); 

            foreach(WebsiteNodeWithNeighbours model in snapshot.Graph)
            {
                foreach(string neighbour in model.Neighbours)
                {
                    data[urlToWebsiteLookup[model.Url]].Add(urlToWebsiteLookup[neighbour]);
                }
            }
            
            var entryWebsite = urlToWebsiteLookup[snapshot.EntryUrl];

            return new WebsiteGraphSnapshot(data, entryWebsite);
        }
    }
}

