using WebCrawler.Models;

namespace WebCrawler.Interfaces;
public interface IExecutionManagerService
{
    ulong EnqueueForCrawl(CrawlInfo crawlInfo);
    WebsiteGraphSnapshot GetGraph(ulong jobId);
    JobStatus GetJobStatus(ulong jobId);
    bool IsValid(ulong jobId);
    Task<bool> StopCrawlingAsync(ulong jobId);
    void WaitForExecutorToFinish(ulong jobId);
}