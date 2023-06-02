using WebCrawler.Models;

namespace WebCrawler.Interfaces
{
    public interface IExecutionManager
    {
        ExecutionManagerConfiguration Config { get; }

        ulong AddToQueueForCrawling(CrawlInfo crawlInfo);
        Task<WebsiteGraphSnapshot?> GetFullGraphAsync(ulong jobId);
        WebsiteGraphSnapshot? GetGraph(ulong jobId);
        Task<bool> StopCrawlingAsync(ulong jobId);
        void RedpillAllCrawlersAndWaitForAllToFinish();
    }
}