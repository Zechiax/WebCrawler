using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IDataService
{
    public ICollection<WebsiteRecord> GetWebsiteRecords();
    
    public WebsiteRecord GetWebsiteRecord(int id);
    
    public void AddWebsiteRecord(WebsiteRecord websiteRecord);
    
    public void UpdateWebsiteRecord(WebsiteRecord websiteRecord);
    
    public void DeleteWebsiteRecord(int id);
    
    public ICollection<Tag> GetTags();
    
    public ExecutorData GetExecution(int id);
    
    public void AddExecution(ExecutorData execution);
    
    public void UpdateExecution(ExecutorData execution);
    
    public void DeleteExecution(int id);
}