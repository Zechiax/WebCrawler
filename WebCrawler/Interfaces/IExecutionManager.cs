using WebCrawler.Models;

namespace WebCrawler.Interfaces
{
    public interface IExecutionManager
    {
        ExecutionManagerConfiguration Config { get; }

        int AddToQueueForCrawling(CrawlInfo crawlInfo);

        /// thread unsafe
        WebsiteGraph GetGraph(int jobId);

        /// full graph - thread safe
        Task<WebsiteGraph> GetFullGraphAsync(int jobId);

        Task<bool> TryStopCrawlingAsync(int jobId);

        void RedpillAllCrawlersAndWaitForAllToFinish();
    }

    public interface IPeriodicManager : IExecutionManager
    {

    }
}
