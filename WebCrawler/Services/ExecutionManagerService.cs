﻿using System.Runtime.CompilerServices;
using WebCrawler.Interfaces;
using WebCrawler.Models;
using WebCrawler.Models.Exceptions;

namespace WebCrawler.Services;

public class ExecutionManagerService : IExecutionManagerService
{
    public ExecutionManagerConfig Config { get; }

    private CrawlerManager crawlerManager;

    private Queue<WebsiteExecutionJob> toCrawlQueue = new();
    private Dictionary<ulong, WebsiteExecutionJob> jobs = new();

    private ulong lastJobId = 0;
    private ILogger logger;

    public ExecutionManagerService(IServiceProvider services, ExecutionManagerConfig config)
    {
        this.logger = services.GetRequiredService<ILogger<ExecutionManagerService>>();
        Config = config;
        crawlerManager = new CrawlerManager(config, services, toCrawlQueue);
        crawlerManager.StartCrawlers();
    }

    public ulong EnqueueForCrawl(CrawlInfo crawlInfo)
    {
        WebsiteExecutionJob job = new(crawlInfo, ++lastJobId);
        jobs[lastJobId] = job;

        EnqueueJob(job);

        return lastJobId;
    }

    public void WaitForExecutionToFinish(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        WebsiteExecutionJob job = jobs[jobId];

        lock (job)
        {
            while (job.JobStatus != JobStatus.Finished)
            {
                Monitor.Wait(job);
            }
        }
    }

    public WebsiteGraphSnapshot GetGraph(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        WebsiteExecutionJob job = jobs[jobId];

        if (job.JobStatus == JobStatus.WaitingInQueue)
        {
            return WebsiteGraphSnapshot.Empty;
        }

        return job.WebsiteGraph?.GetSnapshot() ?? WebsiteGraphSnapshot.Empty;
    }

    public async Task<WebsiteGraphSnapshot> GetFullGraphAsync(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        await Task.CompletedTask;
        WaitForExecutionToFinish(jobId);
        return GetGraph(jobId);
    }

    public async Task<bool> ResetJobAsync(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        bool wasStopped = await StopExecutionAsync(jobId);

        WebsiteExecutionJob job = jobs[jobId];

        lock (job)
        {
            job.WebsiteExecution.Started = null;
            job.WebsiteExecution.Finished = null;
            job.Crawler = null;
            job.JobStatus = JobStatus.WaitingInQueue;
            job.WebsiteGraph = null;
        }

        EnqueueJob(job);

        return wasStopped;
    }

    public async Task<bool> StopExecutionAsync(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        WebsiteExecutionJob job = jobs[jobId];
        lock(job)
        {
            if(job.JobStatus == JobStatus.WaitingInQueue)
            {
                Monitor.Wait(job);
                return true;
            }
        }

        return await job.Crawler!.StopCurrentJob();
    }

    public JobStatus GetJobStatus(ulong jobId)
    {
        if (!IsValid(jobId))
        {
            throw JobIdInvalidException(jobId);
        }

        return jobs[jobId].JobStatus;
    }

    public bool IsValid(ulong jobId)
    {
        return jobs.ContainsKey(jobId);
    }

    private JobIdInvalidException JobIdInvalidException(ulong jobId)
    {
        return new JobIdInvalidException($"Job with {jobId} is invalid.");
    }

    private void EnqueueJob(WebsiteExecutionJob job)
    {
        lock (toCrawlQueue)
        {
            toCrawlQueue.Enqueue(job);
            Monitor.Pulse(toCrawlQueue);
        }
    }
}