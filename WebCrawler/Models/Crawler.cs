using System.Diagnostics;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public class Crawler
{
    private readonly Queue<WebsiteExecutionJob> _toCrawlQueue;
    private WebsiteExecutionJob? _currentJob;
    private readonly IWebsiteProvider _websiteProvider;
    private readonly Task _task;

    private ILogger<Crawler> _logger;
    private readonly IDataService _data;

    public Crawler(ILogger<Crawler> logger, IDataService data, Queue<WebsiteExecutionJob> toCrawlQueue, IWebsiteProvider websiteProvider)
    {
        _data = data;
        _logger = logger;
        this._toCrawlQueue = toCrawlQueue;
        this._websiteProvider = websiteProvider; 

        _task = new Task(CrawlConsumer);
    }

    public void Start()
    {
        _task.Start();
    }

    public Task<bool> StopCurrentJob()
    {
        // No job is running, we have nothing to stop
        if (_currentJob is null)
        {
            return Task.FromResult(false);
        }
        
        lock(_currentJob)
        {
            if (_currentJob.JobStatus is JobStatus.Finished or JobStatus.Stopped)
            {
                return Task.FromResult(false); 
            }

            // -> is active, can't be waiting in queue since this crawler already picked it up
            _currentJob.JobStatus = JobStatus.Stopped;
            Monitor.Wait(_currentJob);
            return Task.FromResult(true);
        }
    }

    private void CrawlConsumer()
    {
        while (true)
        {
            lock (_toCrawlQueue)
            {
                while (_toCrawlQueue.Count == 0)
                {
                    //_logger.LogDebug("{CurrentThreadManagedThreadId}: waiting for jobs to come in",
                    //    Thread.CurrentThread.ManagedThreadId);
                    Monitor.Wait(_toCrawlQueue);
                }

                _currentJob = _toCrawlQueue.Dequeue();
                //_logger.LogDebug("{CurrentThreadManagedThreadId}: dequeuing job: {JobId}",
                //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobId);

                // redpilled
                if (_currentJob is null)
                {
                    //_logger.LogDebug("{CurrentThreadManagedThreadId}: job is a redpill",
                    //    Thread.CurrentThread.ManagedThreadId);
                    return;
                }
            }

            IExecutor executor;

            lock (_currentJob)
            {
                if(_currentJob.JobStatus == JobStatus.Stopped)
                {
                    //_logger.LogDebug(
                    //    "{CurrentThreadManagedThreadId}: job stopped in queue - skipping and pulsing ({JobId})",
                    //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobId);

                    Debug.WriteLine(
                        string.Format("{0}: job stopped in queue - skipping and pulsing ({1})",
                        Thread.CurrentThread.ManagedThreadId, _currentJob.JobId));


                    // Pulses all threads waiting for the job to be stopped, when the job was still in queue.
                    Monitor.PulseAll(_currentJob);
                    continue;
                }

                _currentJob.JobStatus = JobStatus.Active;
                _currentJob.Crawler = this;

                // NOTE: Executor to pass test. DefferedLimited for debugging on client recommended.
                //executor = new DeferredLimitedExecutor(TimeSpan.Zero, 700, _currentJob, _websiteProvider);
                executor = new DeferredLimitedExecutor(TimeSpan.FromSeconds(10), 700, _currentJob, _websiteProvider);
                //executor = new Executor(_currentJob, _websiteProvider);
            }

            //_logger.LogDebug(string.Format("{CurrentThreadManagedThreadId}: start crawling ({JobId})",
            //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobId));

            Debug.WriteLine(string.Format("{0}: start crawling ({1})", Thread.CurrentThread.ManagedThreadId, _currentJob.JobId));

            executor.StartCrawlAsync().Wait();
            
            //_logger.LogDebug("{CurrentThreadManagedThreadId}: finished crawling ({JobId})",
            //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobId);

            Debug.WriteLine(string.Format("{0}: finished crawling ({1})", Thread.CurrentThread.ManagedThreadId, _currentJob.JobId));
            
            //_logger.LogDebug("{CurrentThreadManagedThreadId}: pulsing that job is finished ({JobId})",
            //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobId);

            Debug.WriteLine(string.Format("{0}: pulsing that job is finished ({1})", Thread.CurrentThread.ManagedThreadId, _currentJob.JobId));
            lock (_currentJob)
            {
                // If job was not stopped during crawling, it finished successfuly on it's own.
                if(_currentJob.JobStatus != JobStatus.Stopped)
                {
                    _currentJob.JobStatus = JobStatus.Finished;
                    
                    // Add the job to the database
                    //_logger.LogDebug("{CurrentThreadManagedThreadId}: adding job to database ({JobId})",
                    //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobId);

                    Debug.WriteLine(string.Format("{0}: adding job to database ({1})", Thread.CurrentThread.ManagedThreadId, _currentJob.JobId));
                    
                    _data.AddWebsiteExecution(_currentJob.JobId, _currentJob.WebsiteExecution);
                        
                    //_logger.LogDebug("{CurrentThreadManagedThreadId}: job added to database ({JobId})",
                    //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobId);

                    Debug.WriteLine(string.Format("{0}: job added to database ({1})", Thread.CurrentThread.ManagedThreadId, _currentJob.JobId));
                }
                
                //_logger.LogDebug("{CurrentThreadManagedThreadId}: pulsing that job is over ({JobStatus}) ({JobId})",
                //    Thread.CurrentThread.ManagedThreadId, _currentJob.JobStatus
                //    ,_currentJob.JobId);

                Debug.WriteLine(string.Format("{0}: pulsing that job is over ({1}) ({2})",
                    Thread.CurrentThread.ManagedThreadId, _currentJob.JobStatus
                    ,_currentJob.JobId));

                // Pulses all threads waiting for the job to be stopped, when the job was active.
                Monitor.PulseAll(_currentJob);
            }

            executor.Dispose();
        }
    }
}

