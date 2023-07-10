using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Data;
using WebCrawler.Interfaces;
using WebCrawler.Models.Database;
using WebCrawler.Models.Exceptions;

namespace WebCrawler.Models;

public class DataService : IDataService
{
    private readonly ILogger<DataService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;
    public DataService(IMapper mapper, ILogger<DataService> logger, IServiceProvider serviceProvider)
    {
        _mapper = mapper;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<IList<CrawlInfo>> GetActiveCrawlInfos()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        // Is Active information is stored in the Record, so we need to get all the records
        var recordsDataList = await context.WebsiteRecords
            .AsNoTracking()
            // We want all the tags for each website record
            .Include(wr => wr.Tags)
            // Also if there is an execution, we want to include it too
            .Include(wr => wr.CrawlInfoData)
            // We should be able to suppress nullable warning, as EF Core should handle that
            .ThenInclude(wr => wr.LastExecutionData)
            .Where(wr => wr.IsActive)
            .ToListAsync();
        
        var crawlInfos = recordsDataList.Select(record => _mapper.Map<CrawlInfo>(record.CrawlInfoData)).ToList();
        
        return crawlInfos;
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
        
        var recordsDataList = await context.WebsiteRecords
            .AsNoTracking()
            // We want all the tags for each website record
            .Include(wr => wr.Tags)
            // Also if there is an execution, we want to include it too
            .Include(wr => wr.CrawlInfoData)
            // We should be able to suppress nullable warning, as EF Core should handle that
            .ThenInclude(wr => wr.LastExecutionData)
            .ToListAsync();

        var records = _mapper.Map<IEnumerable<WebsiteRecord>>(recordsDataList);

        return records;
    }

    public async Task<WebsiteRecord> GetWebsiteRecordData(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecordData? recordData = await context.WebsiteRecords
            .AsNoTracking()
            // We want all the tags for each website record
            .Include(wr => wr.Tags)
            // Also if there is an execution, we want to include it too
            .Include(wr => wr.CrawlInfoData)
            .ThenInclude(wr => wr.LastExecutionData)
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (recordData is null)
            throw new KeyNotFoundException($"Website record with id {id} not found.");

        var record = _mapper.Map<WebsiteRecord>(recordData);
        
        return record;
    }

    public async Task<int> AddWebsiteRecord(WebsiteRecord websiteRecord)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        var crawlInfoData = _mapper.Map<CrawlInfoData>(websiteRecord.CrawlInfo);
        var recordData = _mapper.Map<WebsiteRecordData>(websiteRecord);
        
        recordData.CrawlInfoData = crawlInfoData;

        var record = await context.WebsiteRecords.AddAsync(recordData);

        await context.SaveChangesAsync();
        
        return record.Entity.Id;
    }

    public async Task UpdateWebsiteRecord(int id, WebsiteRecord updatedWebsiteRecordData)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
    
        WebsiteRecordData? record = await context.WebsiteRecords
            .Include(wr => wr.CrawlInfoData)
            .ThenInclude(wr => wr.LastExecutionData)
            .Include(wr => wr.Tags) // Include the existing tags
            .FirstOrDefaultAsync(wr => wr.Id == id);
    
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");

        WebsiteExecutionData? execution = record.CrawlInfoData.LastExecutionData;
    
        var recordFromData = _mapper.Map<WebsiteRecord>(updatedWebsiteRecordData);
    
        // Clear the existing tags and add the new tags
        record.Tags.Clear();
        foreach (var tag in recordFromData.Tags)
        {
            var existingTag = await context.Tags.SingleOrDefaultAsync(t => t.Name == tag.Name);
            if (existingTag != null)
            {
                // If the tag already exists in the database, use it
                record.Tags.Add(existingTag);
            }
            else
            {
                // If the tag doesn't exist, create a new one
                record.Tags.Add(tag);
            }
        }

        context.Entry(record).CurrentValues.SetValues(recordFromData);
        context.Entry(record.CrawlInfoData).CurrentValues.SetValues(recordFromData.CrawlInfo);
    
        // In case the execution was not null, we want to keep it
        if (execution is not null)
            record.CrawlInfoData.LastExecutionData = execution;

        await context.SaveChangesAsync();
    }


    public async Task DeleteWebsiteRecord(int id)
    { 
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
    
        WebsiteRecordData? record = await context.WebsiteRecords
            .Include(wr => wr.CrawlInfoData)
            .ThenInclude(wr => wr.LastExecutionData)
            .Include(wr => wr.Tags)
            .FirstOrDefaultAsync(wr => wr.Id == id);
    
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");
    
        context.Tags.RemoveRange(record.Tags);
        if (record.CrawlInfoData.LastExecutionData != null)
            context.Executions.Remove(record.CrawlInfoData.LastExecutionData);
        context.CrawlInfos.Remove(record.CrawlInfoData);

        context.WebsiteRecords.Remove(record);
    
        await context.SaveChangesAsync();
    }


    public async Task AddWebsiteExecution(int recordId, WebsiteExecution websiteExecution)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();

        CrawlInfoData? record = await context.CrawlInfos
            .Include(wr => wr.LastExecutionData)
            .FirstOrDefaultAsync(wr => wr.WebsiteRecordDataId == recordId);

        if (record is null)
            throw new EntryNotFoundException($"Website record with job id {recordId} not found.");
        
        // We have to remove the existing execution, if there is one
        if (record.LastExecutionData != null)
        {
            context.Executions.Remove(record.LastExecutionData);
            await context.SaveChangesAsync();
        }


        var executionData = _mapper.Map<WebsiteExecutionData>(websiteExecution);
        
        record.LastExecutionData = executionData;

        await context.SaveChangesAsync();
    }

    public async Task<bool?> ActivateWebsiteRecord(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecordData? record = await context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");
        
        var active = record.IsActive;
        
        record.IsActive = true;
        
        await context.SaveChangesAsync();
        
        return active;
    }

    public async Task<bool?> DeactivateWebsiteRecord(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();
        
        WebsiteRecordData? record = await context.WebsiteRecords
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");
        
        var active = record.IsActive;
        
        record.IsActive = false;
        
        await context.SaveChangesAsync();
        
        return active;
    }
}