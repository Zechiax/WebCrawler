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
    
    public ExecutorData GetExecution(int id);
    
    public void AddExecution(ExecutorData execution);
    
    public void UpdateExecution(ExecutorData execution);
    
    public void DeleteExecution(int id);
}