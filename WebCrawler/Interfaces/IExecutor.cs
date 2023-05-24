using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IExecutor
{
    void StartCrawl();
    WebsiteModel GetAllCrawledWebsites();
}
