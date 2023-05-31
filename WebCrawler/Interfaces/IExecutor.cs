using System.Text.RegularExpressions;
using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IExecutor
{
    CrawlInfo CrawlInfo { get; }
    Task StartCrawlAsync();
}
