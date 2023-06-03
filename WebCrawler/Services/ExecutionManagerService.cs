using WebCrawler.Interfaces;
using WebCrawler.Models;

namespace WebCrawler.Services;

public class ExecutionManagerService : IExecutionManagerService
{
    public ExecutionManagerConfig Config { get; } 

    private CrawlerManager crawlerManager;

    private Queue<Execution> toCrawlQueue = new();
    private Dictionary<ulong, Execution> jobs = new();

    private ulong lastJobId = 0;
    private ILogger<ExecutionManagerService> logger;

    public ExecutionManagerService(ILogger<ExecutionManagerService> logger, ExecutionManagerConfig config)
    {
        this.logger = logger;
        Config = config;
        crawlerManager = new CrawlerManager(config, toCrawlQueue);
        crawlerManager.StartCrawlers();
    }

    public ulong EnqueueForCrawl(CrawlInfo crawlInfo)
    {
        Execution job = new(crawlInfo, ++lastJobId);
        jobs[lastJobId] = job;

        lock (toCrawlQueue)
        {
            toCrawlQueue.Enqueue(job);
            Monitor.Pulse(toCrawlQueue);
        }

        return lastJobId;
    }

    public void WaitForExecutorToFinish(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdNotPresentException(jobId);
        }

        Execution job = jobs[jobId];

        lock (job)
        {
            while (job.JobStatus != JobStatus.Finished)
            {
                Monitor.Wait(job);
            }
        }
    }

    public WebsiteGraphSnapshot GetGraph(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdNotPresentException(jobId);
        }

        Execution job = jobs[jobId];

        if (job.JobStatus == JobStatus.WaitingInQueue)
        {
            return WebsiteGraphSnapshot.Empty;
        }

        return job.WebsiteGraph!.GetSnapshot();
    }

    public WebsiteGraphSnapshot WaitForGraph(ulong jobId)
    {
        WaitForExecutorToFinish(jobId);
        return GetGraph(jobId);
    }

    public async Task<bool> StopCrawlingAsync(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdNotPresentException(jobId);
        }

        Execution job = jobs[jobId];
        lock (job)
        {
            if (job.JobStatus == JobStatus.WaitingInQueue)
            {
                job.JobStatus = JobStatus.Stopped;
                return true;
            }

            if (job.JobStatus == JobStatus.Finished || job.JobStatus == JobStatus.Stopped)
            {
                return false;
            }
        }

        await job.Crawler!.StopCurrentJob();
        return true;
    }

    public JobStatus GetJobStatus(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdNotPresentException(jobId);
        }

        return jobs[jobId].JobStatus;
    }

    public bool IsValid(ulong jobId)
    {
        return jobs.ContainsKey(jobId);
    }

    private ArgumentException JobIdNotPresentException(ulong jobId)
    {
        return new ArgumentException($"Job with {jobId} already finished and cleaned from job history.");
    }
}
