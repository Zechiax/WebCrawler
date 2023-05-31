using WebCrawler.Models;

namespace WebCrawler.Interfaces
{
    public interface IExecutionManager
    {
        ExecutionManagerConfiguration Config { get; }
        int AddToQueueForCrawling(CrawlInfo crawlInfo);
        Task<WebsiteGraph> WaitFor(int id);
        Task<List<WebsiteGraph>> StopCrawlingForAll();
        /// thread unsafe
        WebsiteGraph GetGraph(int jobId);
        Task<WebsiteGraph> GetFullGraphAsync(int jobId);
        Task<bool> StopCrawlingAsync(int jobId);
        void RedpillAllCrawlersAndWaitForAllToFinish();
    }

    public interface IPeriodicManager : IExecutionManager
    {

    }
}