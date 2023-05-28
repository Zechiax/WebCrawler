using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IDataService
{
    public IEnumerable<WebsiteRecord> GetWebsiteRecords();
    
    public Task<WebsiteRecord> GetWebsiteRecord(int id);
    
    public Task<bool> AddWebsiteRecord(WebsiteRecord websiteRecord);
    
    public Task<bool> UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord);
    
    public Task<bool> DeleteWebsiteRecord(int id);
    
    public ICollection<Tag> GetTags();
    
    public ExecutorData GetExecution(int id);
    
    public void AddExecution(ExecutorData execution);
    
    public void UpdateExecution(ExecutorData execution);
    
    public void DeleteExecution(int id);
}