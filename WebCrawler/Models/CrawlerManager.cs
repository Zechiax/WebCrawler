using WebCrawler.Interfaces;

namespace WebCrawler.Models;

class CrawlerManager
{
    private Crawler[] crawlers;

    public CrawlerManager(ExecutionManagerConfig config, Queue<WebsiteExecutionJob> toCrawlQueue)
    {
        crawlers = new Crawler[config.CrawlersCount];

        for (int index = 0; index < config.CrawlersCount; index++)
        {
            crawlers[index] = new Crawler(toCrawlQueue, (IWebsiteProvider)Activator.CreateInstance(config.TWebsiteProvider)!);
        }
    }

    public void StartCrawlers()
    {
        foreach (Crawler crawler in crawlers)
        {
            crawler.Start();
        }
    }
}

