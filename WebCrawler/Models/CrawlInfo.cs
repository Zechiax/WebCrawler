using System.Text.RegularExpressions;

namespace WebCrawler.Models;

public readonly record struct CrawlInfo
{
    public string EntryUrl { get; init; }
    public TimeSpan Periodicity { get; init; }
    public Regex Regex { get; init; }
}
