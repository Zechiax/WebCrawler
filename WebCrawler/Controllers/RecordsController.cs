using Microsoft.AspNetCore.Mvc;
using WebCrawler.Models;
using WebCrawler.Interfaces;

namespace WebCrawler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecordsController : Controller
    {
        private readonly IDataService _dataService;
        public RecordsController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public IActionResult GetRecords()
        {

            return Ok(_dataService.GetWebsiteRecords());
        }
    }

}
