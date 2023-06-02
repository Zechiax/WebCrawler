using System.Collections.Concurrent;
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
public class ExecutionManager<TWebsiteProvider> : IExecutionManager where TWebsiteProvider : IWebsiteProvider, new()
{
    #region CrawlConsumerInfo

    private class Crawler
    {
        public enum StateEnum
        {
            Active,
            Finished,
            Sleeping,
        }

        public Task Task { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public IExecutor? Executor { get; set; } = null;
        public CrawlJob? Job { get; set; } = null;
        public StateEnum State { get; set; }

        public Crawler(Task task, CancellationTokenSource cancellationTokenSource, StateEnum state)
        {
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
            State = state;
        }
    }

    #endregion

    #region CrawlJob

    private record class CrawlJob
    {
        public CrawlInfo Info { get; init; }
        public ulong JobId { get; init; }
        public bool IsCancelled { get; set; } = false;
        public int? CrawlerIndex { get; set; }

        public CrawlJob(CrawlInfo info, ulong jobId)
        {
            Info = info;
            JobId = jobId;
        }
    }

    #endregion

    public ExecutionManagerConfiguration Config { get; }

    private Queue<CrawlJob?> toCrawlQueue = new();
    private Crawler[] crawlers;

    private ILogger<ExecutionManager<TWebsiteProvider>> logger;

    private readonly ConcurrentDictionary<ulong, WebsiteGraphSnapshot> finishedJobs = new();

    private ulong lastJobId = 0;
    private bool redpilled = false;

    public ExecutionManager(ILogger<ExecutionManager<TWebsiteProvider>> logger, ExecutionManagerConfiguration? config = null)
    {
        this.logger = logger;

        Config = config ?? new ExecutionManagerConfiguration()
        {
            CrawlConsumersCount = 100
        };

        crawlers = new Crawler[Config.CrawlConsumersCount];
        foreach (int index in Enumerable.Range(0, Config.CrawlConsumersCount))
        {
            CancellationTokenSource source = new();

            crawlers[index] = new Crawler(
                Task.Run(() => CrawlConsumer(index, source.Token), source.Token),
                source,
                Crawler.StateEnum.Sleeping);
        }
    }

    public ulong AddToQueueForCrawling(CrawlInfo crawlInfo)
    {
        ThrowIfRedpilled();

        Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: Enquing crawlInfo: {crawlInfo}.");
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
        ThrowIfInvalid(jobId);

        if (TryStopInQueue(jobId))
        {
            // not planned on crawler yet
            return true;
        }

        int? index = GetCrawler(jobId);
        if (index is null)
        {
            // finished or never used jobId
            return false;
        }

        // active on this crawler
        Crawler crawler = crawlers[index.Value];

        crawler.CancellationTokenSource.Cancel();
        try
        {
            await crawler.Task;
        }
        catch (TaskCanceledException)
        {
            // no need to lock crawler, since the other task thread was just cancelled

            crawler.CancellationTokenSource.Dispose();

            crawler.CancellationTokenSource = new CancellationTokenSource();

            // since the crawl task was just cancelled, create a new crawler
            crawler.Task = Task.Run(() => CrawlConsumer(index.Value, crawler.CancellationTokenSource.Token), crawler.CancellationTokenSource.Token);

            lock (toCrawlQueue)
            {
                // at least this one new crawler ready to crawl
                Monitor.Pulse(toCrawlQueue);
            }
        }

        return true;
    }

    public WebsiteGraphSnapshot GetGraph(ulong jobId)
    {
        ThrowIfRedpilled();
        ThrowIfInvalid(jobId);

        if (finishedJobs.TryGetValue(jobId, out WebsiteGraphSnapshot graph))
        {
            return graph;
        }

        int? index = GetCrawler(jobId);
        if (index is null)
        {
            // in queue
            return WebsiteGraphSnapshot.Empty;
        }

        Crawler crawler = crawlers[index.Value];

        lock(crawler)
        {
            return crawler.Executor!.WebsiteExecution.WebsiteGraph.GetSnapshot();
        }
    }

    public WebsiteGraphSnapshot WaitForFullGraph(ulong jobId)
    {
        ThrowIfRedpilled();
        ThrowIfInvalid(jobId);

        if(finishedJobs.TryGetValue(jobId, out WebsiteGraphSnapshot graph))
        {
            return graph;
        }

        int? index = GetCrawler(jobId);
        if(index is null)
        {
            CrawlJob job = toCrawlQueue.FirstOrDefault(job => job?.JobId == jobId)!;

            // wait until some crawler starts processing
            lock(job)
            {
                while(job.CrawlerIndex is null)
                {
                    Monitor.Wait(job);
                }

                index = job.CrawlerIndex;
            }
        }

        Crawler crawler = crawlers[index.Value];

        // wait until crawler finishes processing
        lock(crawler)
        {
            while(crawler.State == Crawler.StateEnum.Active)
            {
                Monitor.Wait(crawler);
            }

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

        Task.WaitAll(crawlers.Select(crawlConsumer => crawlConsumer.Task).ToArray());
    }

    private bool TryStopInQueue(ulong jobId)
    {
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

        return false;
    }

    private int? GetCrawler(ulong jobId)
    {
        for(int index = 0; index < crawlers.Length; ++index)
        {
            Crawler crawler = crawlers[index];
            lock (crawler)
            {
                if (crawler.Job is not null && crawler.Job.JobId == jobId)
                {
                    return index;
                }
            }
        }

        return null; 
    }

    private void CrawlConsumer(int thisCrawlerIndex, CancellationToken ct)
    {
        Debug.WriteLine(thisCrawlerIndex);

        CrawlJob? crawlJob;
        Crawler thisCrawler = crawlers[thisCrawlerIndex];

        lock (toCrawlQueue)
        {
            do
            {
                while (toCrawlQueue.Count == 0)
                {

                    Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: waiting for jobs to come in");

                    lock(thisCrawler)
                    {
                        thisCrawler.State = Crawler.StateEnum.Sleeping;
                    }

                    Monitor.Wait(toCrawlQueue);
                }

                Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: dequing job");

                crawlJob = toCrawlQueue.Dequeue();

                // redpilled
                if (crawlJob is null)
                {
                    Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: job is a redpill");
                    return;
                }

                if (crawlJob.IsCancelled)
                {
                    Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: job cancelled");
                }

            } while (crawlJob.IsCancelled);

            lock(crawlJob)
            {
                crawlJob.CrawlerIndex = thisCrawlerIndex;
                Monitor.PulseAll(crawlJob);
            }
        }

        Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: assigning the job to the crawler");
        lock (thisCrawler)
        {
            thisCrawler.State = Crawler.StateEnum.Active;
            thisCrawler.Job = crawlJob;
            thisCrawler.Executor = new Executor(crawlJob.Info, new TWebsiteProvider());
        }

        Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: start crawling");
        thisCrawler.Executor.StartCrawlAsync(ct).Wait();
        Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: finished crawling");

        Debug.WriteLine($"{nameof(ExecutionManager<TWebsiteProvider>)}: {Thread.CurrentThread.ManagedThreadId}: adding job {thisCrawler.Job.JobId} as finished");
        finishedJobs.TryAdd(thisCrawler.Job.JobId, thisCrawler.Executor.WebsiteExecution.WebsiteGraph.GetSnapshot());

        lock(thisCrawler)
        {
            thisCrawler.State = Crawler.StateEnum.Finished;
            Monitor.PulseAll(thisCrawler);
        }
    }

    private void ThrowIfRedpilled()
    {
        if (redpilled)
        {
            throw new InvalidOperationException($"This {nameof(ExecutionManager<TWebsiteProvider>)} instance is redpilled.");
        }
    }

    private void ThrowIfInvalid(ulong jobId)
    {
        if(jobId > lastJobId)
        {
            throw new ArgumentException($"Invalid jobId: {jobId}");
        }
    }
}
