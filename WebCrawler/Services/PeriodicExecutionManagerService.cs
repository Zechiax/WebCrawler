using System.Dynamic;
using WebCrawler.Interfaces;
using WebCrawler.Models;
using WebCrawler.Models.Exceptions;

namespace WebCrawler.Services;

public class PeriodicExecutionManagerService : IPeriodicExecutionManagerService
{
    public ExecutionManagerConfig Config { get; }

    private ExecutionManagerService executionManager;

    private readonly Dictionary<ulong, PeriodicWebsiteExecutionJob> periodicJobs = new();

    public PeriodicExecutionManagerService(IServiceProvider services, ExecutionManagerConfig? config = null)
    {
        Config = config ?? new ExecutionManagerConfig
        {
            CrawlersCount = 20
        };
        
        executionManager = new ExecutionManagerService(services, Config);
    }

    public void EnqueueForPeriodicCrawl(CrawlInfo crawlInfo, ulong jobId)
    {
        executionManager.EnqueueForCrawl(crawlInfo, jobId);

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
    }

    public async Task<bool> ResetJobAsync(ulong jobId)
    {
        if (executionManager.JobExists(jobId))
        {
            return await executionManager.ResetJobAsync(jobId);
        }

        return false; 
    }

    public Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId)
    {
        return executionManager.GetFullGraphAsync(jobId);
    }

    public WebsiteGraphSnapshot GetLiveGraph(ulong jobId)
    {
        return executionManager.GetLiveGraph(jobId);
    }

    public JobStatus GetJobStatus(ulong jobId)
    {
        return executionManager.GetJobStatus(jobId);
    }

    public Task<bool> StopCurrentExecutionAsync(ulong jobId)
    {
        return executionManager.StopExecutionAsync(jobId);
    }

    public bool StopPeriodicExecution(ulong jobId)
    {
        _ = executionManager.StopExecutionAsync(jobId).Result;

        bool wasPeriodicJobStopped = RemovePeriodicJob(jobId).Result;
        return wasPeriodicJobStopped;
    }

    public void WaitForExecutionToFinish(ulong jobId)
    {
        executionManager.WaitForExecutionToFinish(jobId);
    }

    public bool JobExists(ulong jobId)
    {
        return executionManager.JobExists(jobId);
    }

    private async Task<bool> RemovePeriodicJob(ulong jobId)
    {
        // already removed
        if (!periodicJobs.ContainsKey(jobId))
        {
            return false;
        }

        PeriodicWebsiteExecutionJob job = periodicJobs[jobId];
        periodicJobs.Remove(jobId);

        return await job.Stop();
    }
}
