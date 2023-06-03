using WebCrawler.Interfaces;

namespace WebCrawler.Models;

class CrawlerManager<TWebsiteProvider> where TWebsiteProvider : IWebsiteProvider, new()
{
    private Crawler<TWebsiteProvider>[] crawlers;

    public CrawlerManager(int crawlersCount, Queue<Execution<TWebsiteProvider>> toCrawlQueue)
    {
        crawlers = new Crawler<TWebsiteProvider>[crawlersCount];

        for (int index = 0; index < crawlersCount; index++)
        {
            crawlers[index] = new Crawler<TWebsiteProvider>(toCrawlQueue);
        }
    }

    public void StartCrawlers()
    {
        foreach (Crawler<TWebsiteProvider> crawler in crawlers)
        {
            crawler.Start();
        }
    }
}

