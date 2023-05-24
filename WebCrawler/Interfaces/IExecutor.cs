using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IExecutor
{
    void StartCrawl();
    Website GetAllCrawledWebsites();
}
