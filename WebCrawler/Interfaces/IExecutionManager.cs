using WebCrawler.Models;

namespace WebCrawler.Interfaces
{
    public interface IExecutionManager
    {
        ExecutionManagerConfiguration Config { get; }
        int AddToQueueForCrawling(CrawlInfo crawlInfo);
        Task<WebsiteGraph> WaitFor(int id);
        Task<List<WebsiteGraph>> StopCrawlingForAll();
        Task<List<WebsiteGraph>> StopCrawlingForAllHaving(string thisUrl);
        void RedpillAllCrawlersAndWaitForAllToFinish();
    }
}