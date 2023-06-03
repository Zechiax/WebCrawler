using WebCrawler.Interfaces;

namespace WebCrawler.Models;

record class Execution
{
    public CrawlInfo Info { get; init; }
    public ulong JobId { get; init; }

    public JobStatus JobStatus { get; set; } = JobStatus.WaitingInQueue;
    public Crawler? Crawler { get; set; }
    public WebsiteGraph? WebsiteGraph { get; set; }

    public Execution(CrawlInfo info, ulong jobId)
    {
        Info = info;
        JobId = jobId;
    }
}

