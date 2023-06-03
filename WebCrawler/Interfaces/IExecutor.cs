using System.Text.RegularExpressions;
using WebCrawler.Models;

namespace WebCrawler.Interfaces;

interface IExecutor : IDisposable
{
    WebsiteExecutionJob ExecutionJob { get; }
    Task StartCrawlAsync();
}
