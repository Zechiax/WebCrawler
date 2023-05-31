using System.Text.RegularExpressions;

namespace WebCrawler.Models;

public readonly record struct CrawlInfo
{
    public string EntryUrl { get; init; }
    public Regex Regex { get; init; }
    public TimeSpan Periodicity { get; init; }

    public CrawlInfo(string entryUrl, string regex, TimeSpan periodicity)
    {
        EntryUrl = entryUrl;
        Regex = new Regex(regex);
        Periodicity = periodicity;
    }
}
