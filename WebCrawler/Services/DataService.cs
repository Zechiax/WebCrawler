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
            .Include(wr => wr.CrawlInfo)
            // We should be able to suppress nullable warning, as EF Core should handle that
            .ThenInclude(wr => wr.LastExecution)
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
            .Include(wr => wr.CrawlInfo)
            .ThenInclude(wr => wr.LastExecution)
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new KeyNotFoundException($"Website record with id {id} not found.");
        
        return record;
    }

    public async Task<int> AddWebsiteRecord(WebsiteRecord websiteRecord)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        var record = await context.WebsiteRecords.AddAsync(websiteRecord);
        
        await context.SaveChangesAsync();
        
        return record.Entity.Id;
    }

    public async Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecord)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecord? record = await context.WebsiteRecords
            .Include(wr => wr.CrawlInfo)
            .ThenInclude(wr => wr.LastExecution)
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");

        WebsiteExecution? execution = record.CrawlInfo.LastExecution;
        
        context.Entry(record).CurrentValues.SetValues(updatedWebsiteRecord);
        
        // In case the execution was not null, we want to keep it
        if (execution is not null)
            record.CrawlInfo.LastExecution = execution;

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
        
        CrawlInfo? record = await context.CrawlInfos
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
            .Include(wr => wr.CrawlInfo)
            .FirstOrDefaultAsync(wr => wr.Id == websiteRecordId);
        
        if (record is null)
            throw new EntryNotFoundException("Website record with id {websiteRecordId} not found.");
        
        record.CrawlInfo.JobId = jobId;
        
        await context.SaveChangesAsync();
    }
}