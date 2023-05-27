//#define DEBUG_PRINT

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
    public ExecutionManagerConfiguration Config { get; }

    private Queue<IExecutor?> executorsToRun = new();
    private List<Task> crawlConsumers = new();

    private bool redpilled;

    public ExecutionManager(ExecutionManagerConfiguration? config = null)
    {
        Config = config ?? new ExecutionManagerConfiguration()
        {
            CrawlConsumersCount = 50
        };

        for (int i = 0; i < Config.CrawlConsumersCount; i++)
        {
            crawlConsumers.Add(Task.Run(CrawlConsumer));
        }
    }

    /// <summary>
    /// Adds <paramref name="executor"/> to queue for being crawled as soon as possible.
    /// </summary>
    /// <param name="executor"></param>
    public void AddToQueue(IExecutor executor)
    {
        if (redpilled)
        {
            throw new InvalidOperationException("All consumers are redpilled, can't use this instance anymore.");
        }

        lock (executorsToRun)
        {
#if DEBUG_PRINT
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: enquing executor");
#endif
            executorsToRun.Enqueue(executor);
            Monitor.Pulse(executorsToRun);
        }
    }

    /// <summary>
    /// Redpills all consumers and waits until they all finish.
    /// Beware that after calling this method, this instance becomes unusable.
    /// </summary>
    public void WaitForAllConsumersToFinish()
    {
        RedpillAllCrawlConsumers();
        Task.WaitAll(crawlConsumers.ToArray());
    }

    private void RedpillAllCrawlConsumers()
    {
        redpilled = true;

        lock (executorsToRun)
        {
            for(int i = 0; i < Config.CrawlConsumersCount; i++)
            {
#if DEBUG_PRINT
                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: enquing death");
#endif
                executorsToRun.Enqueue(null);
                Monitor.Pulse(executorsToRun);
            }
        }
    }

    private void CrawlConsumer()
    {
        while (true)
        {
            lock (executorsToRun)
            {
                while (executorsToRun.Count == 0)
                {

#if DEBUG_PRINT
                    Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: waiting");
#endif
                    Monitor.Wait(executorsToRun);
                }

#if DEBUG_PRINT
                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: dequing");
#endif
                IExecutor? executor = executorsToRun.Dequeue();

                // redpill
                if(executor is null)
                {
#if DEBUG_PRINT
                    Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: died");
#endif
                    return;
                }

#if DEBUG_PRINT
                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: crawling");
#endif
                executor.StartCrawlAsync().Wait();
#if DEBUG_PRINT
                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: finished crawling");
#endif
            }
        }
    }
}
