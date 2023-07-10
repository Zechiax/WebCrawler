using WebCrawler.Models;

namespace WebCrawler.Interfaces;

public interface IPeriodicExecutionManagerService
{
    ExecutionManagerConfig Config { get; }
    
    Task InitializeAsync();

    /// <summary>
    /// Enques crawling job according to <paramref name="crawlInfo"/>. 
    /// The job is done periodically according to <see cref="CrawlInfo.Periodicity"/>. 
    /// Returns jobId, which can be later use to query the job information.
    /// </summary>
    void EnqueueForPeriodicCrawl(CrawlInfo crawlInfo, ulong jobId);

    /// <summary>
    /// Returns full graph, after the underlying executor finishes crawling on it's own.
    /// After that, the job is again enqued and again crawled after <see cref="CrawlInfo.Periodicity"/> period elapses. 
    /// </summary>
    Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId);

    /// <summary>
    /// Returns graph how is currently looking.
    /// Isn't blocking.
    /// To make such operation more efficient, it doesn't lock the whole graph, only each node.
    /// Note, that the graph can be reseted again, if the <see cref="CrawlInfo.Periodicity"/> period elapsed.
    /// </summary>
    WebsiteGraphSnapshot GetLiveGraph(ulong jobId);

    /// <summary>
    /// Returs the status of the job.
    /// See <see cref="JobStatus"/> for more information about which states the job can be in.
    /// </summary>
    JobStatus GetJobStatus(ulong jobId);

    /// <summary>
    /// Returns whether this <see cref="IExecutionManagerService"/> keeps track of job with <paramref name="jobId"/>.
    /// </summary>
    bool JobExists(ulong jobId);

    /// <summary>
    /// Stops the execution of the job in current period.
    /// The executor stops crawling on the currently processing node.
    /// The job (with the graph) is stored in the <see cref="IPeriodicExecutionManagerService"/> for later use.
    /// After <see cref="CrawlInfo.Periodicity"/> period elapses, the job is resetted and crawled again.
    /// Note, that this will not stop the job for good, only the current execution.
    /// To stop the job completely, <see cref="StopPeriodicExecution(ulong)"/> method.
    /// </summary>
    Task<bool> StopCurrentExecutionAsync(ulong jobId);

    /// <summary>
    /// Stops the execution of the job and makes sure it will be never repeated again.
    /// The executor stops crawling on the currently processing node.
    /// The job (with the graph) is stored in the <see cref="IPeriodicExecutionManagerService"/> for later use.
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    bool StopPeriodicExecution(ulong jobId);

    /// <summary>
    /// Waits for the execution of the job.
    /// Waits for the underlying executor to traverse the whole graph.
    /// After the job is finished, the job (with the graph) is stored in the <see cref="IExecutionManagerService"/> for later use.
    /// </summary>
    void WaitForExecutionToFinish(ulong jobId);

    Task<bool> ResetJobAsync(ulong jobId);
}
