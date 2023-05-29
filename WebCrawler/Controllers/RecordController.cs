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
            try
            {
                return Ok(_dataService.GetWebsiteRecord(id).Result);
        }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        [HttpPost]
        public IActionResult CreateRecord([FromBody] WebsiteRecord record)
        {
            try
            {
                _dataService.AddWebsiteRecord(record).Wait();
                return Ok();
        }
            catch (Exception)
            {
                return StatusCode(500);
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
            catch (Exception)
        {
                return StatusCode(500);
            }
        }
        [HttpPost]
        [Route("{id:int}/run")]
        public IActionResult RunRecord(int id)
        {
            //TODO: Implement
            return StatusCode(501); //Not Implemented status code
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
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
 }
