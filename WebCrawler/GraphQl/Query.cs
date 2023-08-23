using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;

namespace WebCrawler.GraphQl;

public class Query
{
    public IQueryable<WebPageQl> GetWebPages([Service] CrawlerContext context)
    {
        return context.WebsiteRecords
            .Where(record => record.IsActive)
            .Include(r => r.CrawlInfoData)
            .Include(r => r.Tags)
            .Select(record => new WebPageQl
            {
                Identifier = record.Id,
                Label = record.Label,
                Url = record.CrawlInfoData.EntryUrl,
                Regexp = record.CrawlInfoData.RegexPattern,
                Tags = record.Tags.Select(tag => tag.Name).ToList(),
                Active = record.IsActive
            });   
    }

    public IQueryable<NodeQl> GetNodes([Service] CrawlerContext context, List<int> webPages)
    {
        var records = context.WebsiteRecords
            .AsNoTracking()
            .Include(r => r.CrawlInfoData)
            .ThenInclude(cid => cid.LastExecutionData)
            .Include(r => r.Tags)
            .Where(record => record.CrawlInfoData.LastExecutionData != null)
            .Where(record => webPages.Contains(record.Id));

        var nodes = new List<NodeQl>();

        foreach (var record in records)
        {
            // Should not be null as we filter out the nulls
            var lastExecution = record.CrawlInfoData.LastExecutionData!;

            var json = lastExecution.WebsiteGraphSnapshotJson;

            if (string.IsNullOrEmpty(json))
            {
                continue;
            }

            // BUG: Not working
            var document = JsonSerializer.SerializeToDocument(json);

            // This is array of "nodes"
            var graph = document.RootElement.GetProperty("Graph");

            foreach (var node in graph.EnumerateArray())
            {
                var url = node.GetProperty("Url").GetString();
                var title = node.GetProperty("Title").GetString();
                var crawlTime = node.GetProperty("CrawlTime").GetString();
                var neighbours = node.GetProperty("Neighbours").EnumerateArray().Select(n => n.GetString()).ToList();
                
                var nodeQl = new NodeQl
                {
                    Title = title!,
                    Url = url!,
                    CrawlTime = crawlTime!,
                    Links = neighbours!,
                    WebPageId = record.Id
                };
                
                nodes.Add(nodeQl);
            }
        }

        return nodes.AsQueryable();
    }


        
}

public class WebPageQl
{
    public int Identifier { get; set; }
    public string Label { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Regexp { get; set; } = null!;
    public List<string> Tags { get; set; } = null!;
    public bool Active { get; set; }
}

public class NodeQl
{
    public string Title { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string CrawlTime { get; set; } = null!;
    public List<string> Links { get; set; } = null!;
    public int WebPageId { get; set; }
}
