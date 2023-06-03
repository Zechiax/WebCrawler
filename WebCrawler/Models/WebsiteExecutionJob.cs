namespace WebCrawler.Models;

record class WebsiteExecutionJob
{
    public WebsiteExecution WebsiteExecution { get; }
    public ulong JobId { get; init; }
    public JobStatus JobStatus { get; set; } = JobStatus.WaitingInQueue;
    public Crawler? Crawler { get; set; }
    public WebsiteGraph? WebsiteGraph { get; set; }

    public WebsiteExecutionJob(CrawlInfo info, ulong jobId)
    {
        WebsiteExecution = new WebsiteExecution(info);
        JobId = jobId;
    }
}
