using System.Diagnostics;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public readonly struct ExecutionManagerConfiguration
{
    public int CrawlConsumersCount { get; init; }

    public ExecutionManagerConfiguration() { }
}

public class ExecutionManager : IDisposable
{
    public ExecutionManagerConfiguration Config { get; }

    private Queue<IExecutor?> executorsToRun = new();
    private List<Task> crawlConsumers = new();

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

    public void AddToQueue(IExecutor executor)
    {
        lock (executorsToRun)
        {
            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: enquing executor");
            executorsToRun.Enqueue(executor);
            Monitor.Pulse(executorsToRun);
        }
    }

    public void Dispose()
    {
        lock (executorsToRun)
        {
            for(int i = 0; i < Config.CrawlConsumersCount; i++)
            {
                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: enquing death");
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
                    Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: waiting");
                    Monitor.Wait(executorsToRun);
                }

                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: dequing");
                IExecutor? executor = executorsToRun.Dequeue();

                // redpill
                if(executor is null)
                {
                    Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: died");
                    return;
                }

                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: crawling");
                executor.StartCrawlAsync().Wait();
                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: finished crawling");
            }
        }
    }
}
