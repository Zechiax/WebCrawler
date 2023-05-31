using Newtonsoft.Json;

namespace WebCrawler.Models;

public readonly partial struct WebsiteGraphSnapshot
{
    public static class JsonConverter
    {
        private class Model
        {
            public string Url { get; set; } = null!;
            public string Title { get; set; } = null!;
            public TimeSpan CrawlTime { get; set; }
            public IEnumerable<string> Neighbours { get; set; } = null!;
        }

        public static string Serialize(WebsiteGraphSnapshot graph)
        {
            var websiteVertices = graph.Data.Select(
                websiteNeighbours => new Model
                {
                    Url = websiteNeighbours.Key.Url,
                    Title = websiteNeighbours.Key.Title,
                    CrawlTime = websiteNeighbours.Key.CrawlTime,
                    Neighbours = websiteNeighbours.Value.Select(neighbour => neighbour.Url)
                }
            );
            
            // We add the entry website to the graph, so that we can easily deserialize it later.
            Tuple<string, IEnumerable<Model>> adjacencyList = new(graph.EntryWebsite.Url, websiteVertices);

            return JsonConvert.SerializeObject(adjacencyList);
        }

        public static WebsiteGraphSnapshot Deserialize(string json)
        {
            Tuple<string, IEnumerable<Model>>? models = JsonConvert.DeserializeObject<Tuple<string, IEnumerable<Model>>>(json);

            if(models is null)
            {
                throw new ArgumentException("Can't deserialize from given json.");
            }

            try
            {
                Dictionary<string, Website> urlToWebsiteLookup = models.Item2
                    .ToDictionary(model => model.Url, model => new Website(model.Url) { Title = model.Title, CrawlTime = model.CrawlTime });

                Dictionary<Website, List<Website>> data = urlToWebsiteLookup.Values.ToDictionary(website => website, _ => new List<Website>()); 

                foreach(Model model in models.Item2)
                {
                    foreach(string neighbour in model.Neighbours)
                    {
                        data[urlToWebsiteLookup[model.Url]].Add(urlToWebsiteLookup[neighbour]);
                    }
                }
                
                var entryWebsite = urlToWebsiteLookup[models.Item1];

                return new WebsiteGraphSnapshot(data, entryWebsite);
            }
            catch
            {
                throw new ArgumentException($"The Json representation of {nameof(WebsiteGraphSnapshot)} is invalid. Check that for example each vertex (url) is present only once.");
            }
        }
    }
}

