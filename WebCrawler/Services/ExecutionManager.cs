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

        public CrawlConsumerInfo(Task task, CancellationTokenSource cancellationTokenSource)
        {
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
        }
    }
    #endregion

    public ExecutionManagerConfiguration Config { get; }

    private Queue<CrawlInfo?> toCrawlQueue = new();
    private CrawlConsumerInfo[] crawlConsumers;

    private ILogger<ExecutionManager> logger;

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

    public void AddToQueueForCrawling(CrawlInfo crawlInfo)
    {
        ThrowIfRedpilled();

        lock (toCrawlQueue)
        {
            logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: Enquing crawlInfo: {crawlInfo}.");
            toCrawlQueue.Enqueue(crawlInfo);
            Monitor.Pulse(toCrawlQueue);
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

    public async Task<WebsiteGraph> WaitFor(CrawlInfo crawlInfo)
    {
        ThrowIfRedpilled();

        int index = Array.IndexOf(crawlConsumers, crawlConsumers.First(crawlConsumer => crawlConsumer.Executor!.CrawlInfo == crawlInfo));

        crawlConsumers[index].Task.Wait();
    }

    public async Task<List<WebsiteGraph>> StopCrawlingForAll()
    {
        ThrowIfRedpilled();

        return await StopCrawlingFor(Enumerable.Range(0, crawlConsumers.Length));
    }

    public async Task<List<WebsiteGraph>> StopCrawlingForAllHaving(string thisUrl)
    {
        ThrowIfRedpilled();

        List<int> toStopCrawlingIndeces = new();
        for (int index = 0; index < crawlConsumers.Length; ++index)
        {
            if (crawlConsumers[index].Executor!.CrawlInfo.EntryUrl == thisUrl)
            {
                toStopCrawlingIndeces.Add(index);
            }
        }

        return await StopCrawlingFor(toStopCrawlingIndeces);
    }

    private async Task<List<WebsiteGraph>> StopCrawlingFor(IEnumerable<int> toStopCrawlingIndeces)
    {
        return await Task.Run(() =>
        {
            List<WebsiteGraph> graphs = new();

            foreach (int index in toStopCrawlingIndeces)
            {
                CrawlConsumerInfo consumer = crawlConsumers[index];

                consumer.CancellationTokenSource.Cancel();

                try
                {
                    consumer.Task.Wait();
                }
                catch (TaskCanceledException)
                {
                    graphs.Add(consumer.Executor!.WebsiteExecution.WebsiteGraph);

                    consumer.CancellationTokenSource.Dispose();

                    consumer.CancellationTokenSource = new CancellationTokenSource();
                    consumer.Task = Task.Run(() => CrawlConsumer(index, consumer.CancellationTokenSource.Token), consumer.CancellationTokenSource.Token);

                    lock (toCrawlQueue)
                    {
                        Monitor.Pulse(toCrawlQueue);
                    }
                }
            }

            return graphs;
        });
    }

    private void CrawlConsumer(int index, CancellationToken ct)
    {
        CrawlInfo? crawlInfo;
        lock (toCrawlQueue)
        {
            while (toCrawlQueue.Count == 0)
            {

                logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: waiting");
                Monitor.Wait(toCrawlQueue);
            }

            logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: dequing");
            crawlInfo = toCrawlQueue.Dequeue();
        }

        // redpilled
        if (crawlInfo is null)
        {
            return;
        }

        Executor executor = new(crawlInfo.Value);

        logger.LogTrace($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: crawling");
        executor.StartCrawlAsync(ct).Wait();
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
