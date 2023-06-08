namespace WebCrawler.Models;

public record class WebsiteExecutionJob
{
    public WebsiteExecution WebsiteExecution { get; }
    public CrawlInfo CrawlInfo { get; }
    public ulong JobId { get; init; }
    public JobStatus JobStatus { get; set; } = JobStatus.WaitingInQueue;
    public Crawler? Crawler { get; set; }

    public WebsiteExecutionJob(CrawlInfo info, ulong jobId)
    {
        WebsiteExecution = new WebsiteExecution();
        CrawlInfo = info;
        JobId = jobId;
    }
}
