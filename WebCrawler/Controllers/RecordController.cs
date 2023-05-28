using Microsoft.AspNetCore.Mvc;
using WebCrawler.Interfaces;
using WebCrawler.Models;

namespace WebCrawler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecordController : Controller
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
            return _dataService.GetWebsiteRecord(id).Result switch
            {
                null => NotFound(),
                var record => Ok(record)
            };
        }
        [HttpPost]
        public IActionResult CreateRecordAsync([FromBody] WebsiteRecord record)
        {
            return _dataService.AddWebsiteRecord(record).Result ? Ok() : StatusCode(500);
        }
        [HttpPatch]
        [Route("{id:int}")]
        public IActionResult UpdateRecordAsync(int id, [FromBody] WebsiteRecord record)
        {
            return _dataService.UpdateWebsiteRecord(id, record).Result ? Ok() : StatusCode(500);
        }
        [HttpPost]
        [Route("{id:int}/run")]
        public IActionResult RunRecord(int id)
        {
            var record = new { Id = id, Name = $"Record {id}" };
            return Ok(record);
        }
        [HttpDelete]
        [Route("{id:int}")]
        public IActionResult DeleteRecord(int id)
        {
            return _dataService.DeleteWebsiteRecord(id).Result ? Ok() : StatusCode(500);
        }
    }
 }
