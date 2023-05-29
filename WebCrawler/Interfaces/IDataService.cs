using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IDataService
{
    public Task<IEnumerable<WebsiteRecord>> GetWebsiteRecords();
    
    public Task<WebsiteRecord> GetWebsiteRecord(int id);
    
    public Task AddWebsiteRecord(WebsiteRecord websiteRecord);
    
    public Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord);
    
    public Task DeleteWebsiteRecord(int id);
    
    public ICollection<Tag> GetTags();
    
    public WebsiteExecution GetExecution(int id);
    
    public void AddExecution(WebsiteExecution execution);
    
    public void UpdateExecution(WebsiteExecution execution);
    
    public void DeleteExecution(int id);
}