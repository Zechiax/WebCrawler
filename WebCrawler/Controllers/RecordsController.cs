using Microsoft.AspNetCore.Mvc;
using WebCrawler.Interfaces;

namespace WebCrawler.Controllers;

[ApiController]
[Route("[controller]")]
public class RecordsController : OurController
{
    private readonly IDataService _dataService;
    public RecordsController(IDataService dataService)
    {
        _dataService = dataService;
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
}
