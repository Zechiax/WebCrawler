using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public class DataService : IDataService
{
    private readonly CrawlerContext _context;
    private readonly ILogger<DataService> _logger;
    
    public DataService(CrawlerContext context, ILogger<DataService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task MigrateAsync()
    {
        _logger.LogInformation("Migrating database...");
        await _context.Database.MigrateAsync();
        _logger.LogInformation("Migration complete");
    }

    public async Task<IEnumerable<WebsiteRecord>> GetWebsiteRecords()
    {
        return await _context.WebsiteRecords
            .AsNoTracking()
            // We want all the tags for each website record
            .Include(wr => wr.Tags)
            // Also if there is an execution, we want to include it too
            .Include(wr => wr.LastExecution)
            .ToListAsync();
    }

    public async Task<WebsiteRecord> GetWebsiteRecord(int id)
    {
        WebsiteRecord? record = await _context.WebsiteRecords
            .AsNoTracking()
            // We want all the tags for each website record
            .Include(wr => wr.Tags)
            // Also if there is an execution, we want to include it too
            .Include(wr => wr.LastExecution)
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new KeyNotFoundException($"Website record with id {id} not found.");
        
        return record;
    }

    public async Task AddWebsiteRecord(WebsiteRecord websiteRecord)
    {
        _context.WebsiteRecords.Add(websiteRecord);
        
        await _context.SaveChangesAsync();
    }

    public async Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord)
    {
        WebsiteRecord? record = await _context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new KeyNotFoundException($"Website record with id {id} not found.");
        
        record.Url = updatedWebsiteRecord.Url;
        record.Regex = updatedWebsiteRecord.Regex;
        record.Periodicity = updatedWebsiteRecord.Periodicity;
        record.Label = updatedWebsiteRecord.Label;
        record.IsActive = updatedWebsiteRecord.IsActive;
        record.Tags = updatedWebsiteRecord.Tags;
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteWebsiteRecord(int id)
    { 
        WebsiteRecord? record = await _context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new KeyNotFoundException($"Website record with id {id} not found.");
        
        _context.WebsiteRecords.Remove(record);
        
        await _context.SaveChangesAsync();
    }
}