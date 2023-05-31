//#define DEBUG_PRINT

using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IExecutionManager
{
    ExecutionManagerConfiguration Config { get; }

    void AddToQueueForCrawl(CrawlInfo crawlInfo);
    void WaitForAllConsumersToFinish();
}