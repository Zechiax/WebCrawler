using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using WebCrawler.Interfaces;
using WebCrawler.Models;

namespace WebCrawler.Controllers;

[ApiController]
[Route("[controller]")]
public class RecordController : OurController
{
    private readonly IDataService _dataService;
    private readonly IPeriodicExecutionManagerService _executionManager;

    public RecordController(IDataService dataService, IPeriodicExecutionManagerService executionManager) {
        _dataService = dataService;
        _executionManager = executionManager;
    }

    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetRecord(int id)
    {
        try
        {
            return Ok(_dataService.GetWebsiteRecord(id).Result);
    }
        catch
        {
            return StatusCode(InternalErrorCode);
        }
    }

    [HttpPost]
    public IActionResult CreateRecord([FromBody] string jsonRaw)
    {
        if (string.IsNullOrEmpty(jsonRaw))
        {
            return StatusCode(BadRequestCode);
        }

        var jsonObjScheme = new
        {
            Label = default(string),
            Url = default(string),
            Regex = default(string),
            Periodicity = default(string),
            IsActive = default(string),
            Tags = default(string?[]) 
        };

        try
        {
            var jsonObj = JsonConvert.DeserializeAnonymousType(jsonRaw, jsonObjScheme)!;

            if (string.IsNullOrWhiteSpace(jsonObj.Label) || jsonObj.Label.Length > 30 || jsonObj.Label.Length == 0)
            {
                return StatusCode(BadRequestCode, "Label is invalid.");
            }

            IEnumerable<WebsiteRecord> allRecords = _dataService.GetWebsiteRecords().Result;
            if(allRecords.Any(record => record.Label == jsonObj.Label))
            {
                return StatusCode(BadRequestCode, "Label already present.");
            }

            if (jsonObj.IsActive is not null && jsonObj.IsActive != "on")
            {
                return StatusCode(BadRequestCode, "IsActive is invalid.");
            }

            if (string.IsNullOrWhiteSpace(jsonObj.Url) || !Uri.TryCreate(jsonObj.Url, UriKind.Absolute, out var uriResult) && uriResult?.Scheme == Uri.UriSchemeHttp)
            {
                return StatusCode(BadRequestCode, "Url is invalid.");
            }

            if (string.IsNullOrWhiteSpace(jsonObj.Periodicity) || jsonObj.Periodicity.Length > 15)
            {
                return StatusCode(BadRequestCode, "Periodicity is invalid");
            }

            if (string.IsNullOrWhiteSpace(jsonObj.Regex))
            {
                return StatusCode(BadRequestCode, "Regex is invalid.");
            }

            try
            {
                Regex.Match("", jsonObj.Regex);
            }
            catch (ArgumentException)
            {
                return StatusCode(BadRequestCode, "Regex is invalid.");
            }

            if (jsonObj.Tags is null || jsonObj.Tags.Length >= 50)
            {
                return StatusCode(BadRequestCode, "Tags are invalid.");
            }

            if (jsonObj.Tags.Any(tag => tag is null || tag.Length > 30) || (jsonObj.Tags.ToHashSet().Count != jsonObj.Tags.Length))
            {
                return StatusCode(BadRequestCode, "Tags are invalid.");
            }

            WebsiteRecord record = new(); 
            record.Created = DateTime.UtcNow;

            record.Label = jsonObj.Label;
            record.IsActive = jsonObj.IsActive == "on";
            record.Tags = jsonObj.Tags.Select(tagName => new Tag(tagName!)).ToList();
            record.CrawlInfo = new CrawlInfo(jsonObj.Url, jsonObj.Regex, TimeSpan.FromMinutes(int.Parse(jsonObj.Periodicity)));

            if (record.IsActive)
            {
                record.CrawlInfo.JobId = _executionManager.EnqueueForPeriodicCrawl(record.CrawlInfo);
            }

            _dataService.AddWebsiteRecord(record!).Wait();
            return Ok();
        }
        catch
        {
            return StatusCode(BadRequestCode, "Something went wrong.");
        }
    }

    [HttpPatch]
    [Route("{id:int}")]
    public IActionResult UpdateRecord(int id, [FromBody] WebsiteRecord record)
    {
        try
        {
            _dataService.UpdateWebsiteRecord(id, record).Wait();
            return Ok();
        }
        catch
        {
            return StatusCode(InternalErrorCode);
        }
    }

    [HttpGet]
    [Route("livegraph/websites/{id:int}")]
    public IActionResult LiveGraphWebsites(int id)
    {
        WebsiteRecord record;
        try
        {
            record = _dataService.GetWebsiteRecord(id).Result;
        }
        catch
        {
            return StatusCode(BadRequestCode);
        }

        ulong? jobId = record.CrawlInfo.JobId;
        if(jobId is null)
        {
            return StatusCode(BadRequestCode, "There is no crawling happening under given Website record.");
        }

        try
        {
            WebsiteGraphSnapshot graph = _executionManager.GetLiveGraph(record.CrawlInfo.JobId!.Value);
            string jsonGraph = WebsiteGraphSnapshot.JsonConverter.Serialize(graph);
            return Ok(jsonGraph);
        }
        catch
        {
            return StatusCode(BadRequestCode);
        }
    }

    [HttpGet]
    [Route("livegraph/domains/{id:int}")]
    public IActionResult LiveGraphDomains(int id)
    {
        //TODO: implement
        return Ok();
    }

    [HttpPost]
    [Route("run/{id:int}")]
    public IActionResult RunRecord(int id)
    {
        //TODO: Implement
        return StatusCode(NotImplementedCode);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public IActionResult DeleteRecord(int id)
    {
        try
        {
            _dataService.DeleteWebsiteRecord(id).Wait();
            return Ok();
        }
        catch
        {
            return StatusCode(InternalErrorCode);
        }
    }
}

