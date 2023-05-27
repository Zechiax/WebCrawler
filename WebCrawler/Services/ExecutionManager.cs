using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public readonly struct ExecutionManagerConfiguration
{
    public int CrawlConsumersCount { get; init; }

    public ExecutionManagerConfiguration() { }
}

public static class ExecutionManager
{
    public static ExecutionManagerConfiguration Config { get; set; } = new()
    {
        CrawlConsumersCount = 50
    };

    private static Queue<IExecutor?> executorsToRun = new();
    private static List<Task> consumers = new();

    static ExecutionManager()
    {
        StartAllConsumers();
    }

    public static void AddToQueue(IEnumerable<IExecutor> executors)
    {
        lock (executorsToRun)
        {
            foreach(IExecutor executor in executors)
            {
                AddAndPulse(executor);
            }
        }
    }

    public static void AddToQueue(IExecutor executor)
    {
        lock (executorsToRun)
        {
            AddAndPulse(executor);
        }
    }

    public static void StartAllConsumers()
    {
        // already started
        if(consumers.Count > 0)
        {
            return;
        }

        for (int i = 0; i < Config.CrawlConsumersCount; i++)
        {
            consumers.Add(Task.Run(CrawlConsumer));
        }
    }

    public static void RedpillAllConsumers()
    {
        for(int i = 0; i < Config.CrawlConsumersCount; i++)
        {
            executorsToRun.Enqueue(null);
        }
    }

    private static void AddAndPulse(IExecutor executor)
    {
        executorsToRun.Enqueue(executor);
        Monitor.Pulse(executorsToRun);
    }

    private static void CrawlConsumer()
    {
        while (true)
        {
            lock (executorsToRun)
            {
                while (executorsToRun.Count == 0)
                {
                    Monitor.Wait(executorsToRun);
                }

                IExecutor? executor = executorsToRun.Dequeue();

                // redpill
                if(executor is null)
                {
                    return;
                }

                executor.StartCrawlAsync().Wait();
            }
        }
    }
}
