using System.Diagnostics;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

class Crawler<TWebsiteProvider> where TWebsiteProvider : IWebsiteProvider, new()
{
    private Queue<Execution<TWebsiteProvider>> toCrawlQueue;

    private IExecutor? executor;
    private Execution<TWebsiteProvider> currentJob = null!;

    private Task task;
    private CancellationTokenSource cancellationTokenSource;

    public Crawler(Queue<Execution<TWebsiteProvider>> toCrawlQueue)
    {
        this.toCrawlQueue = toCrawlQueue;

        cancellationTokenSource = new CancellationTokenSource();
        task = new Task(() => CrawlConsumer(cancellationTokenSource.Token), cancellationTokenSource.Token);
    }

    public void Start()
    {
        task.Start();
    }

    public async Task StopCurrentJob()
    {
        cancellationTokenSource.Cancel();

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            currentJob.JobStatus = JobStatus.Stopped;

            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            task = new Task(() => CrawlConsumer(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }
    }

    private void CrawlConsumer(CancellationToken ct)
    {
        while (true)
        {
            bool wasJobStopped;

            lock (toCrawlQueue)
            {
                do
                {
                    while (toCrawlQueue.Count == 0)
                    {

                        Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: waiting for jobs to come in");
                        Monitor.Wait(toCrawlQueue);
                    }

                    currentJob = toCrawlQueue.Dequeue();
                    Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: dequing job: {currentJob?.JobId}");

                    // redpilled
                    if (currentJob is null)
                    {
                        Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: job is a redpill");
                        return;
                    }

                    wasJobStopped = currentJob.JobStatus == JobStatus.Stopped;

                    if (wasJobStopped)
                    {
                        Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: job stopped ({currentJob.JobId})");
                    }

                } while (wasJobStopped);
            }

            executor = new Executor(currentJob.Info, new TWebsiteProvider());

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: pulsing that job is active ({currentJob.JobId})");

            lock (currentJob)
            {
                currentJob.WebsiteGraph = executor.WebsiteExecution.WebsiteGraph;
                currentJob.JobStatus = JobStatus.Active;
                Monitor.PulseAll(currentJob);
            }

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : start crawling ({currentJob.JobId})");

            executor.StartCrawlAsync(ct).Wait();

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : finished crawling ({currentJob.JobId})");

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : pulsing that job is finished ({currentJob.JobId})");
            lock (currentJob)
            {
                currentJob.JobStatus = JobStatus.Finished;
                Monitor.PulseAll(currentJob);
            }
        }
    }
}

