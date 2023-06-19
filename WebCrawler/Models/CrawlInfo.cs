namespace WebCrawler.Models;

public class CrawlInfo
{
    public ulong? JobId { get; set; }
    public string EntryUrl { get; init; } = null!;
    public string RegexPattern { get; init; } = ".*";
    public TimeSpan Periodicity { get; init; }
    public WebsiteExecution? LastExecution { get; set; }
    public int WebsiteRecordId { get; set; }
    
    public CrawlInfo(string entryUrl, string regexPattern, TimeSpan periodicity)
    {
        EntryUrl = entryUrl;
        RegexPattern = regexPattern;
        Periodicity = periodicity;
    }

    public CrawlInfo()
    {
    }
}