using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCrawler.Models;


public class Website
{
    public string Url { get; set; } = string.Empty;
    public TimeSpan CrawlTime { get; set; } = TimeSpan.Zero;
    public string Title { get; set; } = string.Empty;
    public List<Website> OutgoingWebsites { get; set; } = new();
}
