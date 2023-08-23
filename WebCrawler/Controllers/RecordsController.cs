using Microsoft.AspNetCore.Mvc;
using WebCrawler.Interfaces;
using WebCrawler.Models.Exceptions;
using WebCrawler.Models;
using System.Diagnostics.CodeAnalysis;

namespace WebCrawler.Controllers;

public class RecordsController : OurControllerBase
{
    private readonly IDataService _dataService;
    private readonly IPeriodicExecutionManagerService _executionManager;
    public RecordsController(IDataService dataService, IPeriodicExecutionManagerService executionManager)
    {
        _dataService = dataService;
        _executionManager = executionManager;
    }

    [HttpGet]
    public IActionResult GetRecords()
    {
        try
        {
            return Ok(_dataService.GetWebsiteRecords().Result);
        }
        catch
        {
            return StatusCode(InternalErrorCode);
        }
    }

    [HttpGet]
    [Route("statuses")]
    public IActionResult GetStatuses([FromQuery(Name = "ids")] int[] ids)
    {
        string[] statuses = new string[ids.Length];

        try
        {
            for (int i = 0; i < ids.Length; i++)
            {
                int id = ids[i];
                if (TryGetWebsiteRecord(id, out WebsiteRecord? record))
                {
                    ulong jobId = (ulong)record.Id;

                    if (_executionManager.JobExists(jobId))
                    {
                        JobStatus status = _executionManager.GetJobStatus(jobId);
                        statuses[i] = "{" + $"{jobId} : " + status.ToString() + "}";
                    }
                    else
                    {
                        statuses[i] = "{" + $"{jobId} : Stopped" + "}";
                    }
                }
            }
        }
        catch (EntryNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch
        {
            return StatusCode(InternalErrorCode);
        }

        return Ok(statuses);
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
