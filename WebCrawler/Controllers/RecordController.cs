using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using WebCrawler.Interfaces;
using WebCrawler.Models;
using WebCrawler.Models.Database;
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
        if (!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return StatusCode(InternalErrorCode);
        }
        
        // BUG: Currently, client side needs an website execution to not be null, so we'll just return an empty one
        record.CrawlInfo.LastExecution ??= new WebsiteExecution()
        {
            Started = DateTime.Now,
            Finished = DateTime.Now,
        };

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

        var crawlInfo = returnedRecord.CrawlInfo;
        
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
        ValidationResult result = await _recordValidator.ValidateAsync(record);

        if (!result.IsValid)
        {
            return BadRequest(JsonConvert.SerializeObject(result.Errors));
        }

        if (!TryGetWebsiteRecord(id, out WebsiteRecord? websiteRecordData))
        {
            return StatusCode(InternalErrorCode);
        }

        var jobId = (ulong)id;
        if (_executionManager.JobExists(jobId))
        {
            _executionManager.StopPeriodicExecution(jobId);
        }

        var websiteRecord = _mapper.Map<WebsiteRecord>(websiteRecordData);

        websiteRecord.Label = record.Label;
        websiteRecord.IsActive = record.IsActive;
        websiteRecord.Tags = record.Tags.Select(tagName => new Tag(tagName)).ToList();
        websiteRecord.CrawlInfo = new CrawlInfo(record.Url, record.Regex, TimeSpan.FromMinutes(record.Periodicity));

        await _dataService.UpdateWebsiteRecord(id, websiteRecord);

        var returnedRecord = await _dataService.GetWebsiteRecordData(id);

        var crawlInfo = returnedRecord.CrawlInfo;

        if (websiteRecord.IsActive)
        {
            _executionManager.EnqueueForPeriodicCrawl(crawlInfo, jobId);
        }

        return Ok(id);
    }

    [HttpGet]
    [Route("livegraph/domains/{id:int}")]
    public IActionResult LiveGraphDomains(int id)
    {
        WebsiteGraphSnapshot? graph = GetLiveGraph(id);

        if(graph is null)
        {
            return StatusCode(BadRequestCode, "There is no graph");
        }

        string graphJson = WebsiteGraphSnapshot.JsonConverterToDomainView.Serialize(graph.Value);
        return Ok(graphJson);
    }

    [HttpGet]
    [Route("livegraph/websites/{id:int}")]
    public IActionResult LiveGraphWebsites(int id)
    {
        WebsiteGraphSnapshot? graph = GetLiveGraph(id);

        if(graph is null)
        {
            return StatusCode(BadRequestCode, "There is no graph");
        }
        
        string graphJson = WebsiteGraphSnapshot.JsonConverter.Serialize(graph.Value);
        return Ok(graphJson);
    }

    private WebsiteGraphSnapshot? GetLiveGraph(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return null; 
        }

        ulong jobId = (ulong)record.CrawlInfo.WebsiteRecordId;

        try
        {
            WebsiteGraphSnapshot graph = _executionManager.GetLiveGraph(jobId);
            return graph; 
        }
        catch
        {
            return null; 
        }
    }

    [HttpPost]
    [Route("rerun/{id:int}")]
    public async Task<IActionResult> RerunExecutionOnRecord(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return StatusCode(BadRequestCode, $"Website record with given id: {id} not found.");
        }
        
        var crawlInfo = _mapper.Map<CrawlInfo>(record!.CrawlInfo);

        ulong jobId = (ulong)crawlInfo.WebsiteRecordId;
        if (_executionManager.JobExists(jobId))
        {
            await _executionManager.ResetJobAsync(jobId);
        }

        record.IsActive = true;
        await _dataService.UpdateWebsiteRecord(id, record);

        return Ok();
    }

    [HttpPost]
    [Route("run/{id:int}")]
    public async Task<IActionResult> RunExecutionOnRecord(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return StatusCode(BadRequestCode, $"Website record with given id: {id} not found.");
        }
        
        var crawlInfo = _mapper.Map<CrawlInfo>(record.CrawlInfo);
        
        await _dataService.ActivateWebsiteRecord(id);

        _executionManager.EnqueueForPeriodicCrawl(crawlInfo, (ulong)crawlInfo.WebsiteRecordId);
        return Ok();
    }

    [HttpPost]
    [Route("stop/{id:int}")]
    public async Task<IActionResult> StopExecutionOnRecord(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return StatusCode(BadRequestCode, $"Website record with given id: {id} not found.");
        }
        
        await _dataService.DeactivateWebsiteRecord(id);

        ulong jobId = (ulong) id;
        
        if (!_executionManager.JobExists(jobId))
        {
            return Ok("Job already stopped.");
        }
        
        bool didIJustStopped = _executionManager.StopPeriodicExecution(jobId);

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
            if(TryGetWebsiteRecord(id, out WebsiteRecord? record))
            {
                ulong jobId = (ulong)record.Id;
                if(_executionManager.JobExists(jobId))
                {
                    _executionManager.StopPeriodicExecution(jobId);
                }
            }

            await _dataService.DeleteWebsiteRecord(id);

            return Ok();
        }
        catch (EntryNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch
        {
            return StatusCode(InternalErrorCode);   
        }
    }

    private bool TryGetWebsiteRecord(int id, [NotNullWhen(true)] out WebsiteRecord? record)
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

