using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IPeriodicExecutionManagerService
{
    ExecutionManagerConfig Config { get; }

    ulong EnqueueForPeriodicCrawl(CrawlInfo crawlInfo);
    Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId);
    WebsiteGraphSnapshot GetGraph(ulong jobId);
    JobStatus GetJobStatus(ulong jobId);
    bool IsValid(ulong jobId);
    Task<bool> StopCurrentExecutionAsync(ulong jobId);
    bool StopPeriodicExecutionAsync(ulong jobId);
    void WaitForExecutionToFinish(ulong jobId);
}
