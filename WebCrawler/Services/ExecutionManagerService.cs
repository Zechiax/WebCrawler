using System.Runtime.CompilerServices;
using WebCrawler.Interfaces;
using WebCrawler.Models;

namespace WebCrawler.Services;

public class ExecutionManagerService : IExecutionManagerService
{
    public ExecutionManagerConfig Config { get; }

    private CrawlerManager crawlerManager;

    private Queue<WebsiteExecutionJob> toCrawlQueue = new();
    private Dictionary<ulong, WebsiteExecutionJob> jobs = new();

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
        WebsiteExecutionJob job = new(crawlInfo, ++lastJobId);
        jobs[lastJobId] = job;

        lock (toCrawlQueue)
        {
            toCrawlQueue.Enqueue(job);
            Monitor.Pulse(toCrawlQueue);
        }

        return lastJobId;
    }

    public void WaitForExecutionToFinish(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        WebsiteExecutionJob job = jobs[jobId];

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
            throw JobIdInvalidException(jobId);
        }

        WebsiteExecutionJob job = jobs[jobId];

        if (job.JobStatus == JobStatus.WaitingInQueue)
        {
            return WebsiteGraphSnapshot.Empty;
        }

        return job.WebsiteGraph?.GetSnapshot() ?? WebsiteGraphSnapshot.Empty;
    }

    public async Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId)
    {
        await Task.CompletedTask;
        WaitForExecutionToFinish(jobId);
        return GetGraph(jobId);
    }

    public async Task<bool> StopExecutionAsync(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        WebsiteExecutionJob job = jobs[jobId];
        lock(job)
        {
            if(job.JobStatus == JobStatus.WaitingInQueue)
            {
                Monitor.Wait(job);
                return true;
            }
        }

        return await job.Crawler!.StopCurrentJob();
    }

    public JobStatus GetJobStatus(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        return jobs[jobId].JobStatus;
    }

    public bool IsValid(ulong jobId)
    {
        return jobs.ContainsKey(jobId);
    }

    private ArgumentException JobIdInvalidException(ulong jobId)
    {
        return new ArgumentException($"Job with {jobId} is invalid.");
    }
}
