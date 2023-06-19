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

    public async Task<WebsiteRecordData> GetWebsiteRecordData(int id)
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

        return recordData;
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
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");

        WebsiteExecutionData? execution = record.CrawlInfoData.LastExecutionData;
        
        var recordFromData = _mapper.Map<WebsiteRecord>(updatedWebsiteRecordData);
        
        context.Entry(record).CurrentValues.SetValues(recordFromData);
        
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
            .FirstOrDefaultAsync(wr => wr.Id == id);
        
        if (record is null)
            throw new EntryNotFoundException($"Website record with id {id} not found.");
        
        context.WebsiteRecords.Remove(record);
        
        await context.SaveChangesAsync();
    }

    public async Task AddWebsiteExecution(int recordId, WebsiteExecution websiteExecution)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrawlerContext>();

        CrawlInfoData? record = await context.CrawlInfos
            .FirstOrDefaultAsync(wr => wr.WebsiteRecordDataId == recordId);

        if (record is null)
            throw new EntryNotFoundException($"Website record with job id {recordId} not found.");

        var executionData = _mapper.Map<WebsiteExecutionData>(websiteExecution);
        
        record.LastExecutionData = executionData;

        await context.SaveChangesAsync();
    }

}