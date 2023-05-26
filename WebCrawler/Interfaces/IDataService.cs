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
    
    public Execution GetExecution(int id);
    
    public void AddExecution(Execution execution);
    
    public void UpdateExecution(Execution execution);
    
    public void DeleteExecution(int id);
}