using WebCrawler.Interfaces;
using WebCrawler.Models;
using WebCrawler.Models.Exceptions;

namespace WebCrawler.Services;

public class PeriodicExecutionManagerService : IPeriodicExecutionManagerService
{
    public ExecutionManagerConfig Config { get; }

    private ExecutionManagerService executionManager;

    private readonly Dictionary<ulong, PeriodicWebsiteExecutionJob> periodicJobs = new();

    public PeriodicExecutionManagerService(ILogger logger, ExecutionManagerConfig config)
    {
        Config = config;

        executionManager = new ExecutionManagerService(logger, config);
    }

    public ulong EnqueueForPeriodicCrawl(CrawlInfo crawlInfo)
    {
        ulong jobId;

        lock (executionManager)
        {
            jobId = executionManager.EnqueueForCrawl(crawlInfo);
        }

        periodicJobs[jobId] = new PeriodicWebsiteExecutionJob(
            crawlInfo.Periodicity,
            () =>
            {
                lock (executionManager)
                {
                    _ = executionManager.ResetJobAsync(jobId).Result;
                }
            }
        );

        return jobId;
    }

    public Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId)
    {
        lock (executionManager)
        {
            return executionManager.GetFullGraphAsync(jobId);
        }
    }

    public WebsiteGraphSnapshot GetGraph(ulong jobId)
    {
        lock (executionManager)
        {
            return executionManager.GetGraph(jobId);
        }
    }

    public JobStatus GetJobStatus(ulong jobId)
    {
        lock (executionManager)
        {
            return executionManager.GetJobStatus(jobId);
        }
    }

    public Task<bool> StopCurrentExecutionAsync(ulong jobId)
    {
        lock (executionManager)
        {
            return executionManager.StopExecutionAsync(jobId);
        }
    }

    public bool StopPeriodicExecutionAsync(ulong jobId)
    {
        lock (executionManager)
        {
            _ = executionManager.StopExecutionAsync(jobId).Result;
        }

        bool wasPeriodicJobStopped = RemovePeriodicJob(jobId).Result;
        return wasPeriodicJobStopped;
    }

    public void WaitForExecutionToFinish(ulong jobId)
    {
        lock (executionManager)
        {
            executionManager.WaitForExecutionToFinish(jobId);
        }
    }

    public bool IsValid(ulong jobId)
    {
        return executionManager.IsValid(jobId);
    }

    private async Task<bool> RemovePeriodicJob(ulong jobId)
    {
        if (!periodicJobs.ContainsKey(jobId))
        {
            throw new JobIdInvalidException($"Periodic job with {jobId} doesn't exist.");
        }

        PeriodicWebsiteExecutionJob job = periodicJobs[jobId];
        periodicJobs.Remove(jobId);

        return await job.Stop();
    }
}
