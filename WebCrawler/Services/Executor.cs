using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public class ExecutorImplementationNameShouldBeChangedImNotSureWhatTheNameShouldBeRightNow : Executor, IExecutor
{
    // root of the tree
    private Website entryWebsite;

    public void StartCrawl()
    {
        // crawling
    }

    public Website GetAllCrawledWebsites()
    {
        return entryWebsite;
    }
}
