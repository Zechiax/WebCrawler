using System.Text.RegularExpressions;

namespace WebCrawler.Models;

public readonly record struct CrawlInfo
{
    public string EntryUrl { get; init; }
    public Regex Regex { get; init; }
    public TimeSpan Periodicity { get; init; }

    public CrawlInfo(string entryUrl, Regex regex, TimeSpan periodicity)
    {
        EntryUrl = entryUrl;
        Regex = regex;
        Periodicity = periodicity;
    }
}
