using WebCrawler.Model;

namespace WebCrawler.Interfaces;

public interface IExecutor
{
    void StartCrawl();
    WebsiteModel GetAllCrawledWebsites();
}
