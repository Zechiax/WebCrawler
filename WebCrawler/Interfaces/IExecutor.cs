using System.Text.RegularExpressions;
using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IExecutor
{
    string EntryUrl { get; }
    Regex Regex { get; }
    TimeSpan Periodicity { get; }

    Task StartCrawlAsync();
}
