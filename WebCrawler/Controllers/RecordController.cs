using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using WebCrawler.Interfaces;
using WebCrawler.Models;
using WebCrawler.Models.Exceptions;

namespace WebCrawler.Controllers;

public class RecordController : OurControllerBase
{
    private readonly IDataService _dataService;
    private readonly IPeriodicExecutionManagerService _executionManager;
    private readonly IValidator<CreateRecordRequestDto> _recordValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<RecordController> _logger;
    
    public RecordController(ILogger<RecordController> logger, IMapper mapper, IValidator<CreateRecordRequestDto> recordValidator,IDataService dataService, IPeriodicExecutionManagerService executionManager) {
        _mapper = mapper;
        _dataService = dataService;
        _executionManager = executionManager;
        _recordValidator = recordValidator;
        _logger = logger;
    }

    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetRecord(int id)
    {
        if (!TryGetWebsiteRecord(id, out WebsiteRecordData? record))
        {
            return StatusCode(InternalErrorCode);
        }

        return Ok(record);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecord([FromBody] CreateRecordRequestDto record)
    {
        ValidationResult result = await _recordValidator.ValidateAsync(record);
        
        if (!result.IsValid)
        {
            return BadRequest(JsonConvert.SerializeObject(result.Errors));
        }
        
        var websiteRecord = new WebsiteRecord
        {
            Label = record.Label,
            IsActive = record.IsActive,
            Tags = record.Tags.Select(tagName => new Tag(tagName)).ToList(),
            CrawlInfo = new CrawlInfo(record.Url, record.Regex, TimeSpan.FromMinutes(record.Periodicity))
        };

        // We have to use the return crawl info data, as the job id is not set yet
        var id = await _dataService.AddWebsiteRecord(websiteRecord);
        
        var returnedRecord = await _dataService.GetWebsiteRecordData(id);
        
        var crawlInfo = _mapper.Map<CrawlInfo>(returnedRecord.CrawlInfoData);
        
        _logger.LogInformation("Created record with id {ReturnedRecordId}", returnedRecord.Id);
        if (returnedRecord.IsActive)
        {
            _executionManager.EnqueueForPeriodicCrawl(crawlInfo, (ulong)returnedRecord.Id);
        }

        return Ok(id);
    }

    [HttpPatch]
    [Route("{id:int}")]
    public async Task<IActionResult> UpdateRecord(int id, [FromBody] CreateRecordRequestDto record)
    {
        if (!TryGetWebsiteRecord(id, out WebsiteRecordData? websiteRecordData))
        {
            return StatusCode(InternalErrorCode);
        }

        ulong? jobId = websiteRecordData!.CrawlInfoData.JobId;
        if (!(jobId is null))
        {
            _executionManager.StopPeriodicExecution(websiteRecordData.CrawlInfoData.JobId!.Value);
        }

        var websiteRecord = _mapper.Map<WebsiteRecord>(websiteRecordData);

        ValidationResult result = await _recordValidator.ValidateAsync(record);

        if (!result.IsValid)
        {
            return BadRequest(JsonConvert.SerializeObject(result.Errors));
        }

        websiteRecord.Label = record.Label;
        websiteRecord.IsActive = record.IsActive;
        websiteRecord.Tags = record.Tags.Select(tagName => new Tag(tagName)).ToList();
        websiteRecord.CrawlInfo = new CrawlInfo(record.Url, record.Regex, TimeSpan.FromMinutes(record.Periodicity));

        await _dataService.UpdateWebsiteRecord(id, websiteRecord);

        if (websiteRecord.IsActive)
        {
            _executionManager.EnqueueForPeriodicCrawl(websiteRecord.CrawlInfo, (ulong)id);
        }

        return Ok();
    }

    [HttpGet]
    [Route("livegraph/domains/{id:int}")]
    public IActionResult LiveGraphDomains(int id)
    {
        string? graphJson = GetLiveGraphJson(id);

        if(graphJson is null)
        {
            return StatusCode(BadRequestCode, "There is no graph");
        }
        
        return Ok(graphJson);
    }

    [HttpGet]
    [Route("livegraph/websites/{id:int}")]
    public IActionResult LiveGraphWebsites(int id)
    {
        string? graphJson = GetLiveGraphJson(id);

        if(graphJson is null)
        {
            return StatusCode(BadRequestCode, "There is no graph");
        }
        
        return Ok(graphJson);
    }

    private string? GetLiveGraphJson(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecordData? record))
        {
            return null; 
        }

        ulong jobId = (ulong)record.CrawlInfoData.WebsiteRecordDataId;

        try
        {
            WebsiteGraphSnapshot graph = _executionManager.GetLiveGraph(jobId);
            return WebsiteGraphSnapshot.JsonConverter.Serialize(graph);
        }
        catch
        {
            return record.CrawlInfoData.LastExecutionData?.WebsiteGraphSnapshotJson;
        }
    }

    [HttpPost]
    [Route("run/{id:int}")]
    public IActionResult RunExecutionOnRecord(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecordData? record))
        {
            return StatusCode(BadRequestCode, $"Website record with given id: {id} not found.");
        }
        
        var crawlInfo = _mapper.Map<CrawlInfo>(record!.CrawlInfoData);
        
        _executionManager.EnqueueForPeriodicCrawl(crawlInfo, (ulong)crawlInfo.WebsiteRecordId);
        return Ok();
    }

    [HttpPost]
    [Route("stop/{id:int}")]
    public IActionResult StopExecutionOnRecord(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecordData? record))
        {
            return StatusCode(BadRequestCode, $"Website record with given id: {id} not found.");
        }

        ulong? jobId = record!.CrawlInfoData.JobId;
        if(jobId is null)
        {
            return StatusCode(BadRequestCode, $"Can't stop execution for record with given id: {id}, since jobId is not set, meaning there is no active crawler for this website record.");
        }

        bool didIJustStopped = _executionManager.StopPeriodicExecution(record.CrawlInfoData.JobId!.Value);

        if (!didIJustStopped)
        {
            return Ok("Job already stopped.");
        }

        return Ok("Job successfuly stopped.");
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> DeleteRecord(int id)
    {
        try
        {
            await _dataService.DeleteWebsiteRecord(id);
            return Ok();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch
        {
            return StatusCode(InternalErrorCode);   
        }
    }

    private bool TryGetWebsiteRecord(int id, [NotNullWhen(true)] out WebsiteRecordData? record)
    {
        record = null;

        try
        {
            record = _dataService.GetWebsiteRecordData(id).Result;
            return true;
        }
        catch (EntryNotFoundException)
        {
            return false;
        }
    }
}

