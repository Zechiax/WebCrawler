using WebCrawler.Interfaces;

namespace WebCrawler.Models;

class CrawlerManager
{
    private Crawler[] crawlers;

    public CrawlerManager(ExecutionManagerConfig config, IServiceProvider services, Queue<WebsiteExecutionJob> toCrawlQueue)
    {
        crawlers = new Crawler[config.CrawlersCount];
        var dataService = services.GetRequiredService<IDataService>();

        for (int index = 0; index < config.CrawlersCount; index++)
        {
            var logger = services.GetRequiredService<ILogger<Crawler>>();
            crawlers[index] = new Crawler(logger, dataService, toCrawlQueue, (IWebsiteProvider)Activator.CreateInstance(config.TWebsiteProvider)!);
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

