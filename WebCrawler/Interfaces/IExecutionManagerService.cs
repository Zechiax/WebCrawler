using WebCrawler.Models;

namespace WebCrawler.Interfaces;
public interface IExecutionManagerService
{
    ExecutionManagerConfig Config { get; }
    ulong EnqueueForCrawl(CrawlInfo crawlInfo);
    Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId);
    WebsiteGraphSnapshot GetGraph(ulong jobId);
    Task<bool> StopExecutionAsync(ulong jobId);
    void WaitForExecutionToFinish(ulong jobId);
    JobStatus GetJobStatus(ulong jobId);
    bool IsValid(ulong jobId);
}