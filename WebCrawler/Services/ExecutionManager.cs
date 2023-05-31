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
/// Producent is <see cref="AddToQueue(IExecutor)"/> API.
/// Consumers are threads that will do the crawling.
/// </summary>
public class ExecutionManager
{
    #region CrawlConsumerInfo
    private class CrawlConsumerInfo
    {
        public Task Task { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public CrawlInfo? CrawlInfo { get; set; } = null;

        public CrawlConsumerInfo(Task task, CancellationTokenSource cancellationTokenSource)
        {
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
        }
    }
    #endregion

    public ExecutionManagerConfiguration Config { get; }

    private Queue<CrawlInfo> toCrawlQueue = new();
    private CrawlConsumerInfo[] crawlConsumers;

    private Serilog.ILogger logger;

    public ExecutionManager(Serilog.ILogger logger, ExecutionManagerConfiguration? config = null)
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
        lock (toCrawlQueue)
        {
            logger.Information($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: Enquing crawlInfo: {crawlInfo}.");
            toCrawlQueue.Enqueue(crawlInfo);
            Monitor.Pulse(toCrawlQueue);
        }
    }

    public async Task<int> StopCrawlingForAll()
    {
        return await StopCrawlingFor(Enumerable.Range(0, crawlConsumers.Length));
    }

    public async Task<int> StopCrawlingForAllHaving(string thisUrl)
    {
        List<int> toStopCrawlingIndeces = new();
        for (int index = 0; index < crawlConsumers.Length; ++index)
        {
            if (crawlConsumers[index].CrawlInfo!.Value.EntryUrl == thisUrl)
            {
                toStopCrawlingIndeces.Add(index);
            }
        }

        return await StopCrawlingFor(toStopCrawlingIndeces);
    }

    private async Task<int> StopCrawlingFor(IEnumerable<int> toStopCrawlingIndeces)
    {
        return await Task.Run(() =>
        {
            int stopped = 0;

            foreach(int index in toStopCrawlingIndeces)
            {
                CrawlConsumerInfo consumer = crawlConsumers[index];

                try
                {
                    consumer.CancellationTokenSource.Cancel();
                    consumer.Task.Wait();
                }
                catch
                {
                    stopped++;

                    consumer.CancellationTokenSource.Dispose();

                    consumer.CancellationTokenSource = new CancellationTokenSource();
                    consumer.Task = Task.Run(() => CrawlConsumer(index, consumer.CancellationTokenSource.Token));

                    lock (toCrawlQueue)
                    {
                        Monitor.Pulse(toCrawlQueue);
                    }
                }
            }

            return stopped;
        });
    }

    private void CrawlConsumer(int index, CancellationToken ct)
    {
        while (true)
        {
            lock (toCrawlQueue)
            {
                while (toCrawlQueue.Count == 0)
                {

                    logger.Information($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: waiting");
                    Monitor.Wait(toCrawlQueue);
                }

                logger.Information($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: dequing");
                CrawlInfo? crawlInfo = toCrawlQueue.Dequeue();

                Executor executor = new(crawlInfo.Value);

                logger.Information($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: crawling");
                executor.StartCrawlAsync(ct).Wait();
                logger.Information($"{nameof(ExecutionManager)}: {Thread.CurrentThread.ManagedThreadId}: finished crawling");
            }
        }
    }
}
