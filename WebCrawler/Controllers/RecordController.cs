using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
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

    public RecordController(IValidator<CreateRecordRequestDto> recordValidator,IDataService dataService, IPeriodicExecutionManagerService executionManager) {
        _dataService = dataService;
        _executionManager = executionManager;
        _recordValidator = recordValidator;
    }

    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetRecord(int id)
    {
        if (!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return StatusCode(InternalErrorCode);
        }

        return Ok(_dataService.GetWebsiteRecord(id).Result);
    }

    [HttpPost]
    public IActionResult CreateRecord([FromBody] CreateRecordRequestDto record)
    {
        ValidationResult result = _recordValidator.Validate(record);
        
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

        _dataService.AddWebsiteRecord(websiteRecord).Wait();
        
        if (websiteRecord.IsActive)
        {
            websiteRecord.CrawlInfo.JobId = _executionManager.EnqueueForPeriodicCrawl(websiteRecord.CrawlInfo);
        }

        return Ok(websiteRecord.Id);
    }

    [HttpPatch]
    [Route("{id:int}")]
    public async Task<IActionResult> UpdateRecord(int id, [FromBody] WebsiteRecord record)
    {
        try
        {
            await _dataService.UpdateWebsiteRecord(id, record);
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

    [HttpGet]
    [Route("livegraph/domains/{id:int}")]
    public IActionResult LiveGraphDomains(int id)
    {
        WebsiteGraphSnapshot? graph = GetLiveGraph(id);

        if(graph is null)
        {
            return StatusCode(BadRequestCode, "There is no graph");
        }

        string jsonGraph = WebsiteGraphSnapshot.JsonConverterToDomainView.Serialize(graph.Value);
        return Ok(jsonGraph);
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

        string jsonGraph = WebsiteGraphSnapshot.JsonConverter.Serialize(graph.Value);
        return Ok(jsonGraph);
    }

    private WebsiteGraphSnapshot? GetLiveGraph(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return null; 
        }

        ulong? jobId = record!.CrawlInfo.JobId;
        if(jobId is null)
        {
            return null; 
        }

        try
        {
            WebsiteGraphSnapshot graph = _executionManager.GetLiveGraph(record.CrawlInfo.JobId!.Value);
            return graph;
        }
        catch
        {
            return null;
        }
    }

    [HttpPost]
    [Route("run/{id:int}")]
    public IActionResult RunExecutionOnRecord(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return StatusCode(BadRequestCode, $"Website record with given id: {id} not found.");
        }

        record!.CrawlInfo.JobId = _executionManager.EnqueueForPeriodicCrawl(record.CrawlInfo);
        return Ok();
    }

    [HttpPost]
    [Route("stop/{id:int}")]
    public IActionResult StopExecutionOnRecord(int id)
    {
        if(!TryGetWebsiteRecord(id, out WebsiteRecord? record))
        {
            return StatusCode(BadRequestCode, $"Website record with given id: {id} not found.");
        }

        ulong? jobId = record!.CrawlInfo.JobId;
        if(jobId is null)
        {
            return StatusCode(BadRequestCode, $"Can't stop execution for record with given id: {id}, since jobId is not set, meaning there is no active crawler for this website record.");
        }

        bool didIJustStopped = _executionManager.StopPeriodicExecution(record.CrawlInfo.JobId!.Value);

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

    private bool TryGetWebsiteRecord(int id, out WebsiteRecord? record)
    {
        record = null;

        try
        {
            record = _dataService.GetWebsiteRecord(id).Result;
            return true;
        }
        catch (EntryNotFoundException)
        {
            return false;
        }
    }
}

