using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public class Executor : ExecutorModel, IExecutor
{
    // root of the tree
    private WebsiteModel entryWebsite;

    public Executor(WebsiteRecordModel entryWebsite) : base(entryWebsite) { }

    public void StartCrawl()
    {
        // crawling
    }

    public WebsiteModel GetAllCrawledWebsites()
    {
        return entryWebsite;
    }
}
