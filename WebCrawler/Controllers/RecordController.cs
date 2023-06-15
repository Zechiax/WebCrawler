using Microsoft.AspNetCore.Http;
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
    /**/
    private readonly IDataService _dataService;
    public RecordController(IDataService dataService) {
        _dataService = dataService;
    }
    /**/

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

        var jsonObjDefinition = new
        {
            Label = default(string),
            Url = default(string),
            Regex = default(string),
            Periodicity = default(string),
            IsActive = default(string),
        };

        try
        {
            var jsonObj = JsonConvert.DeserializeAnonymousType(jsonRaw, jsonObjDefinition)!;

            if(string.IsNullOrWhiteSpace(jsonObj.Label) || jsonObj.Label.Length > 30 || jsonObj.Label.Length == 0)
            {
                return StatusCode(BadRequestCode);
            }

            if(jsonObj.IsActive is not null && jsonObj.IsActive != "on")
            {
                return StatusCode(BadRequestCode);
            }

            if(string.IsNullOrWhiteSpace(jsonObj.Url) || !Uri.TryCreate(jsonObj.Url, UriKind.Absolute, out var uriResult) && uriResult?.Scheme == Uri.UriSchemeHttp)
            {
                return StatusCode(BadRequestCode);
            }

            if(string.IsNullOrWhiteSpace(jsonObj.Periodicity) || jsonObj.Periodicity.Length > 30)
            {
                return StatusCode(BadRequestCode);
            }

            if(string.IsNullOrWhiteSpace(jsonObj.Regex))
            {
                return StatusCode(BadRequestCode);
            }

            try
            {
                Regex.Match("", jsonObj.Regex);
            }
            catch (ArgumentException)
            {
                return StatusCode(BadRequestCode);
            }

            WebsiteRecord record = new(); 
            record.Created = DateTime.UtcNow;

            record.Label = jsonObj.Label;
            record.IsActive = jsonObj.IsActive == "on";
            record.CrawlInfo = new CrawlInfo(jsonObj.Url, jsonObj.Regex, TimeSpan.FromMinutes(int.Parse(jsonObj.Periodicity)));

            _dataService.AddWebsiteRecord(record!).Wait();
            return Ok();
        }
        catch
        {
            return StatusCode(BadRequestCode);
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
    [HttpPost]
    [Route("{id:int}/run")]
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

