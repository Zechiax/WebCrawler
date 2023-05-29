using WebCrawler.Data;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public class DataService : IDataService
{
    private readonly CrawlerContext _context;
    
    public DataService(CrawlerContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WebsiteRecord>> GetWebsiteRecords()
    {
        throw new NotImplementedException();
    }

    public async Task<WebsiteRecord> GetWebsiteRecord(int id)
    {
        throw new NotImplementedException();
    }

    public async Task AddWebsiteRecord(WebsiteRecord websiteRecord)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteWebsiteRecord(int id)
    {
        throw new NotImplementedException();
    }
}