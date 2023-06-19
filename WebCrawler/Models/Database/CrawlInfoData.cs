using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using WebCrawler.Models.Database;

namespace WebCrawler.Models;

public record CrawlInfoData
{
    [Key]
    public int Id { get; set; }
    public ulong? JobId { get; set; }
    public string EntryUrl { get; init; } = null!;
    public string RegexPattern { get; init; } = ".*";
    public TimeSpan Periodicity { get; init; }
    public WebsiteExecutionData? LastExecution { get; set; }
    public int WebsiteRecordId { get; set; }

    /// <summary>
    /// Constructor for EF Core
    /// </summary>
    public CrawlInfoData()
    { }
}
