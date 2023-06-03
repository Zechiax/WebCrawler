using System.Diagnostics;
using System.Runtime.CompilerServices;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

class Crawler
{
    private Queue<WebsiteExecutionJob> toCrawlQueue;

    private WebsiteExecutionJob currentJob = null!;

    private readonly IWebsiteProvider websiteProvider;

    private Task task;

    public Crawler(Queue<WebsiteExecutionJob> toCrawlQueue, IWebsiteProvider websiteProvider)
    {
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
            }

            IExecutor executor;

            lock (currentJob)
            {
                if(currentJob.JobStatus == JobStatus.Stopped)
                {
                    Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: job stopped in queue - skipping and pulsing ({currentJob.JobId})");

                    // Pulses all threads waiting for the job to be stopped, when the job was still in queue.
                    Monitor.PulseAll(currentJob);
                    goto tryDequingAgain;
                }

                currentJob.JobStatus = JobStatus.Active;
                currentJob.Crawler = this;
                executor = new Executor(currentJob, websiteProvider);
            }

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : start crawling ({currentJob.JobId})");

            executor.StartCrawlAsync().Wait();

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : finished crawling ({currentJob.JobId})");

            Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} : pulsing that job is finished ({currentJob.JobId})");
            lock (currentJob)
            {
                // If job was not stopped during crawling, it finished successfuly on it's own.
                if(currentJob.JobStatus != JobStatus.Stopped)
                {
                    currentJob.JobStatus = JobStatus.Finished;
                }

                Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: pulsing that job is over (stopped or finished) ({currentJob.JobId})");

                // Pulses all threads waiting for the job to be stopped, when the job was active.
                Monitor.PulseAll(currentJob);
            }

            executor?.Dispose();
        }
    }
}

