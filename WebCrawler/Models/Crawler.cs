using System.Diagnostics;
using System.Runtime.CompilerServices;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public class Crawler
{
    private Queue<WebsiteExecutionJob> toCrawlQueue;

    private WebsiteExecutionJob currentJob = null!;

    private readonly IWebsiteProvider websiteProvider;

    private Task task;

    private ILogger<Crawler> _logger;
    private readonly IDataService _data;

    public Crawler(ILogger<Crawler> logger, IDataService data, Queue<WebsiteExecutionJob> toCrawlQueue, IWebsiteProvider websiteProvider)
    {
        data = data;
        _logger = logger;
        this.toCrawlQueue = toCrawlQueue;
        this.websiteProvider = websiteProvider; 

        task = new Task(CrawlConsumer);
    }

    public void Start()
    {
        task.Start();
    }

    public async Task<bool> StopCurrentJob()
    {
        await Task.CompletedTask;

        lock(currentJob)
        {
            if (currentJob.JobStatus == JobStatus.Finished || currentJob.JobStatus == JobStatus.Stopped)
            {
                return false; 
            }

            // -> is active, can't be waiting in queue since this crawler already picked it up
            currentJob.JobStatus = JobStatus.Stopped;
            Monitor.Wait(currentJob);
            return true;
        }
    }

    private void CrawlConsumer()
    {
        tryDequingAgain:
        while (true)
        {
            lock (toCrawlQueue)
            {
                while (toCrawlQueue.Count == 0)
                {
                    _logger.LogDebug("{CurrentThreadManagedThreadId}: waiting for jobs to come in",
                        Thread.CurrentThread.ManagedThreadId);
                    Monitor.Wait(toCrawlQueue);
                }

                currentJob = toCrawlQueue.Dequeue();
                _logger.LogDebug("{CurrentThreadManagedThreadId}: dequeuing job: {JobId}",
                    Thread.CurrentThread.ManagedThreadId, currentJob?.JobId);

                // redpilled
                if (currentJob is null)
                {
                    _logger.LogDebug("{CurrentThreadManagedThreadId}: job is a redpill",
                        Thread.CurrentThread.ManagedThreadId);
                    return;
                }
            }

            IExecutor executor;

            lock (currentJob)
            {
                if(currentJob.JobStatus == JobStatus.Stopped)
                {
                    _logger.LogDebug(
                        "{CurrentThreadManagedThreadId}: job stopped in queue - skipping and pulsing ({JobId})",
                        Thread.CurrentThread.ManagedThreadId, currentJob.JobId);

                    // Pulses all threads waiting for the job to be stopped, when the job was still in queue.
                    Monitor.PulseAll(currentJob);
                    goto tryDequingAgain;
                }

                currentJob.JobStatus = JobStatus.Active;
                currentJob.Crawler = this;
                executor = new Executor(currentJob, websiteProvider);
            }
            
            _logger.LogDebug("{CurrentThreadManagedThreadId}: start crawling ({JobId})",
                Thread.CurrentThread.ManagedThreadId, currentJob.JobId);

            executor.StartCrawlAsync().Wait();
            
            _logger.LogDebug("{CurrentThreadManagedThreadId}: finished crawling ({JobId})",
                Thread.CurrentThread.ManagedThreadId, currentJob.JobId);
            
            _logger.LogDebug("{CurrentThreadManagedThreadId}: pulsing that job is finished ({JobId})",
                Thread.CurrentThread.ManagedThreadId, currentJob.JobId);
            lock (currentJob)
            {
                // If job was not stopped during crawling, it finished successfuly on it's own.
                if(currentJob.JobStatus != JobStatus.Stopped)
                {
                    currentJob.JobStatus = JobStatus.Finished;
                    
                    // Add the job to the database
                    _logger.LogDebug("{CurrentThreadManagedThreadId}: adding job to database ({JobId})",
                        Thread.CurrentThread.ManagedThreadId, currentJob.JobId);
                    
                    _data.AddWebsiteExecution(currentJob.JobId, currentJob.WebsiteExecution);
                    
                    _logger.LogDebug("{CurrentThreadManagedThreadId}: job added to database ({JobId})",
                        Thread.CurrentThread.ManagedThreadId, currentJob.JobId);
                }
                
                _logger.LogDebug("{CurrentThreadManagedThreadId}: pulsing that job is over ({JobStatus}) ({JobId})",
                    Thread.CurrentThread.ManagedThreadId, currentJob.JobStatus
                    ,currentJob.JobId);

                // Pulses all threads waiting for the job to be stopped, when the job was active.
                Monitor.PulseAll(currentJob);
            }

            executor?.Dispose();
        }
    }
}

