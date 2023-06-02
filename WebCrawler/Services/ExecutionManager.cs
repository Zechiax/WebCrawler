using System.Diagnostics;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public readonly struct ExecutionManagerConfiguration
{
    public int CrawlConsumersCount { get; init; }

    public ExecutionManagerConfiguration() { }
}

/// <summary>
/// Manages executors as producent consumer problem (https://en.wikipedia.org/wiki/Producer%E2%80%93consumer_problem).
/// Producent is <see cref="AddToQueueForCrawling(CrawlInfo)"/> API.
/// Consumers are threads that will do the crawling.
/// </summary>
public class ExecutionManager : IExecutionManager
{
    #region CrawlConsumerInfo
    private class CrawlConsumerInfo
    {
        public Task Task { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public IExecutor? Executor { get; set; } = null;
        public CrawlJob? Job { get; set; } = null;

        public CrawlConsumerInfo(Task task, CancellationTokenSource cancellationTokenSource)
        {
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
        }
    }
    #endregion

    #region CrawlJob

    private record class CrawlJob
    {
        public CrawlInfo Info { get; init; }
        public ulong JobId { get; init; }
        public bool IsCancelled { get; set; } = false;

        public CrawlJob(CrawlInfo info, ulong jobId)
        {
            Info = info;
            JobId = jobId;
        }
    }

    #endregion

    public ExecutionManagerConfiguration Config { get; }

    private Queue<CrawlJob?> toCrawlQueue = new();
    private CrawlConsumerInfo[] crawlConsumers;

    private ILogger<ExecutionManager> logger;

    private ulong lastJobId = 0;
    private bool redpilled = false;

    public ExecutionManager(ILogger<ExecutionManager> logger, ExecutionManagerConfiguration? config = null)
    {
        this.logger = logger;

        Config = config ?? new ExecutionManagerConfiguration()
        {
            CrawlConsumersCount = Environment.ProcessorCount * 5
        };

        crawlConsumers = new CrawlConsumerInfo[Config.CrawlConsumersCount];
        for (int i = 0; i < Config.CrawlConsumersCount; i++)
        {
            CancellationTokenSource source = new();
            crawlConsumers[i] = new CrawlConsumerInfo(
                Task.Run(() => CrawlConsumer(i, source.Token), source.Token),
                source);
        }
    }

    public ulong AddToQueueForCrawling(CrawlInfo crawlInfo)
    {
        ThrowIfRedpilled();

        logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: Enquing crawlInfo: {crawlInfo}.");
        lock (toCrawlQueue)
        {
            toCrawlQueue.Enqueue(new CrawlJob(crawlInfo, ++lastJobId));
            Monitor.Pulse(toCrawlQueue);
        }

        return lastJobId;
    }

    public async Task<bool> StopCrawlingAsync(ulong jobId)
    {
        ThrowIfRedpilled();

        lock (toCrawlQueue)
        {
            foreach (CrawlJob? job in toCrawlQueue)
            {
                if (job is null)
                {
                    // redpill, at the end of queue
                    return false;
                }

                if (job.JobId == jobId)
                {
                    job.IsCancelled = true;
                    return true;
                }
            }
        }

        CrawlConsumerInfo? crawler = GetCrawler(jobId);
        if (crawler is null)
        {
            return false;
        }

        int index = Array.IndexOf(crawlConsumers, crawler);

        crawler.CancellationTokenSource.Cancel();
        try
        {
            await crawler.Task;
        }
        catch (TaskCanceledException)
        {
            crawler.CancellationTokenSource.Dispose();

            crawler.CancellationTokenSource = new CancellationTokenSource();
            crawler.Task = Task.Run(() => CrawlConsumer(index, crawler.CancellationTokenSource.Token), crawler.CancellationTokenSource.Token);

            lock (toCrawlQueue)
            {
                Monitor.Pulse(toCrawlQueue);
            }
        }

        return true;
    }

    public WebsiteGraphSnapshot? GetGraph(ulong jobId)
    {
        ThrowIfRedpilled();

        CrawlConsumerInfo? crawler = GetCrawler(jobId);
        if (crawler is null)
        {
            return null;
        }

        lock(crawler)
        {
            return crawler.Executor!.WebsiteExecution.WebsiteGraph.GetSnapshot();
        }
    }

    public async Task<WebsiteGraphSnapshot?> GetFullGraphAsync(ulong jobId)
    {
        ThrowIfRedpilled();

        CrawlConsumerInfo? crawler = GetCrawler(jobId);
        if(crawler is null)
        {
            return null;
        }

        await crawler.Task;
        lock(crawler)
        {
            return crawler.Executor!.WebsiteExecution.WebsiteGraph.GetSnapshot();
        }
    }

    public void RedpillAllCrawlersAndWaitForAllToFinish()
    {
        ThrowIfRedpilled();

        lock (toCrawlQueue)
        {
            for (int i = 0; i < Config.CrawlConsumersCount; ++i)
            {
                toCrawlQueue.Enqueue(null);
            }

            redpilled = true;
        }

        Task.WaitAll(crawlConsumers.Select(crawlConsumer => crawlConsumer.Task).ToArray());
    }

    private CrawlConsumerInfo? GetCrawler(ulong jobId)
    {
        return crawlConsumers
        .FirstOrDefault(crawler =>
        {
            lock (crawler)
            {
                return crawler.Job is not null && crawler.Job.JobId == jobId;
            }
        });
    }

    private void CrawlConsumer(int index, CancellationToken ct)
    {
        CrawlJob? crawlJob;
        CrawlConsumerInfo crawler;
        lock (toCrawlQueue)
        {
            while (toCrawlQueue.Count == 0)
            {

                logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: waiting");
                Monitor.Wait(toCrawlQueue);
            }

            logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: dequing");
            crawlJob = toCrawlQueue.Dequeue();

            // redpilled
            if (crawlJob is null)
            {
                return;
            }

            crawler = crawlConsumers[index];
            lock (crawler)
            {
                crawler.Job = crawlJob;
                crawler.Executor = new Executor(crawlJob.Info);
            }
        }

        logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: crawling");
        crawler.Executor.StartCrawlAsync(ct).Wait();
        logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: finished crawling");
    }

    private void ThrowIfRedpilled()
    {
        if (redpilled)
        {
            throw new InvalidOperationException($"This {nameof(ExecutionManager)} instance is redpilled.");
        }
    }
}
