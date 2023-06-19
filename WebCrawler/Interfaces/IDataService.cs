using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IDataService
{
    public Task MigrateAsync();
    public Task<IEnumerable<WebsiteRecord>> GetWebsiteRecords();
    
    public Task<WebsiteRecord> GetWebsiteRecord(int id);
    
    public Task<int> AddWebsiteRecord(WebsiteRecord websiteRecord);
    
    public Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord);
    
    public Task DeleteWebsiteRecord(int id);
    public Task AddWebsiteExecution(int recordId, WebsiteExecution websiteExecution);
}