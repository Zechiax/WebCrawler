using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IDataService
{
    public Task<IList<CrawlInfo>> GetActiveCrawlInfos();
    public Task MigrateAsync();
    public Task<IEnumerable<WebsiteRecord>> GetWebsiteRecords();
    
    public Task<WebsiteRecord> GetWebsiteRecordData(int id);
    
    public Task<int> AddWebsiteRecord(WebsiteRecord websiteRecord);
    
    public Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord);
    
    public Task DeleteWebsiteRecord(int id);
    public Task AddWebsiteExecution(int recordId, WebsiteExecution websiteExecution);
    
    /// <summary>
    /// Activates a website record.
    /// </summary>
    /// <param name="id"></param>
    /// <returns> Previous state of the record. </returns>
    public Task<bool?> ActivateWebsiteRecord(int id);
    
    /// <summary>
    /// Deactivates a website record.
    /// </summary>
    /// <param name="id"></param>
    /// <returns> Previous state of the record. </returns>
    public Task<bool?> DeactivateWebsiteRecord(int id);
    
    
}