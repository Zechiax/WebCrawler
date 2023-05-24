namespace WebCrawler.Models;

public record class WebsiteRecordModel
{
    public string Url { get; set; }
    public string Regex { get; set; }
    public TimeSpan Periodicity { get; set; }
    public string Label { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastCrawl { get; set; }
    public DateTime Created { get; set; }
    public List<string> Tags { get; set; } = new();
}
