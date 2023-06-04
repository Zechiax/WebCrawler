using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;
using WebCrawler.Interfaces;
using WebCrawler.Models.Exceptions;

namespace WebCrawler.Models;

public class DataService : IDataService
{
    private readonly ILogger<DataService> _logger;
    private readonly IServiceProvider _serviceProvider;
    public DataService(ILogger<DataService> logger, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task MigrateAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        _logger.LogInformation("Migrating database...");
        await context.Database.MigrateAsync();
        _logger.LogInformation("Migration complete");
    }

    public async Task<IEnumerable<WebsiteRecord>> GetWebsiteRecords()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        return await context.WebsiteRecords
            .AsNoTracking()
            // We want all the tags for each website record
            .Include(wr => wr.Tags)
            // Also if there is an execution, we want to include it too
            .Include(wr => wr.LastExecution)
            .ToListAsync();
    }

    public async Task<WebsiteRecord> GetWebsiteRecord(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecord? record = await context.WebsiteRecords
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
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        context.WebsiteRecords.Add(websiteRecord);
        
        await context.SaveChangesAsync();
    }

    public async Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecord? record = await context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new KeyNotFoundException($"Website record with id {id} not found.");
        
        record.Url = updatedWebsiteRecord.Url;
        record.Regex = updatedWebsiteRecord.Regex;
        record.Periodicity = updatedWebsiteRecord.Periodicity;
        record.Label = updatedWebsiteRecord.Label;
        record.IsActive = updatedWebsiteRecord.IsActive;
        record.Tags = updatedWebsiteRecord.Tags;
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteWebsiteRecord(int id)
    { 
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecord? record = await context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");
        
        context.WebsiteRecords.Remove(record);
        
        await context.SaveChangesAsync();
    }

    public async Task AddWebsiteExecution(ulong jobId, WebsiteExecution websiteExecution)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecord? record = await context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.JobId == jobId);
        
        if (record is null)
            throw new EntryNotFoundException($"Website record with job id {jobId} not found.");
        
        record.LastExecution = websiteExecution;
        
        await context.SaveChangesAsync();
    }

    public async Task UpdateJobId(int websiteRecordId, ulong? jobId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecord? record = await context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.Id == websiteRecordId);
        
        if (record is null)
            throw new EntryNotFoundException("Website record with id {websiteRecordId} not found.");
        
        record.JobId = jobId;
        
        await context.SaveChangesAsync();
    }
}