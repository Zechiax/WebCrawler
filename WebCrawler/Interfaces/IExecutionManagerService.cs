using WebCrawler.Models;

namespace WebCrawler.Interfaces;
public interface IExecutionManagerService
{
    ExecutionManagerConfig Config { get; }

    /// <summary>
    /// Enques crawling job according to <paramref name="crawlInfo"/>. 
    /// Returns jobId, which can be later use to query the job information.
    /// </summary>
    void EnqueueForCrawl(CrawlInfo crawlInfo, ulong jobId);

    /// <summary>
    /// Returns full graph, after the underlying executor finishes crawling on it's own.
    /// </summary>
    Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId);

    /// <summary>
    /// Returns graph how is currently looking.
    /// Isn't blocking.
    /// To make such operation more efficient, it doesn't lock the whole graph, only each node.
    /// </summary>
    WebsiteGraphSnapshot GetLiveGraph(ulong jobId);

    /// <summary>
    /// Stops the execution of the job.
    /// The executor stops crawling on the currently processing node.
    /// The job (with the graph) is stored in the <see cref="IExecutionManagerService"/> for later use.
    /// </summary>
    Task<bool> StopExecutionAsync(ulong jobId);

    /// <summary>
    /// Waits for the execution of the job.
    /// Waits for the underlying executor to traverse the whole graph.
    /// After the job is finished, the job (with the graph) is stored in the <see cref="IExecutionManagerService"/> for later use.
    /// </summary>
    void WaitForExecutionToFinish(ulong jobId);

    /// <summary>
    /// Resets the job.
    /// Meaning, the job is stopped, the graph and all job information is reseted (lost) and the job is enqued again.
    /// </summary>
    Task<bool> ResetJobAsync(ulong jobId);

    /// <summary>
    /// Returs the status of the job.
    /// See <see cref="JobStatus"/> for more information about which states the job can be in.
    /// </summary>
    JobStatus GetJobStatus(ulong jobId);

    /// <summary>
    /// Returns whether this <see cref="IExecutionManagerService"/> keeps track of job with <paramref name="jobId"/>.
    /// </summary>
    bool JobExists(ulong jobId);
}