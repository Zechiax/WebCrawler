using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebCrawler.Models;

public record CrawlInfo
{
    [Key]
    public int Id { get; set; }
    public ulong? JobId { get; set; }
    public string EntryUrl { get; init; } = null!;
    public string RegexPattern { get; init; } = ".*";
    public TimeSpan Periodicity { get; init; }
    public WebsiteExecution? LastExecution { get; set; }
    public int WebsiteRecordId { get; set; }

    /// <summary>
    /// Constructor for EF Core
    /// </summary>
    public CrawlInfo()
    { }
    
    public CrawlInfo(string entryUrl, string regexPattern, TimeSpan periodicity)
    {
        EntryUrl = entryUrl;
        RegexPattern = regexPattern;
        Periodicity = periodicity;
    }
}
