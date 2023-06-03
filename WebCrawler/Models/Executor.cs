using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

/// <summary>
/// Crawls all websites.
/// </summary>
class Executor : IExecutor
{
    public WebsiteExecutionJob ExecutionJob { get; }

    private readonly IWebsiteProvider websiteProvider;
    private Dictionary<string, Website> VisitedUrlToWebsite = new();
    private bool disposed = false;

    public Executor(WebsiteExecutionJob execution, IWebsiteProvider? websiteProvider = null)
    {
        this.websiteProvider = websiteProvider ?? new WebsiteProvider();

        ExecutionJob = execution;
        ExecutionJob.WebsiteGraph = new WebsiteGraph(new Website(ExecutionJob.WebsiteExecution.Info.EntryUrl));
    }

    ~Executor()
    {
        if (!disposed)
        {
            Dispose();
        }
    }

    public async Task StartCrawlAsync()
    {
        ExecutionJob.WebsiteExecution.Started = DateTime.Now;

        await CrawlAsync(ExecutionJob.WebsiteGraph!.EntryWebsite)
        .ContinueWith(_ =>
        {
            ExecutionJob.WebsiteExecution.Finished = DateTime.Now;
        });
    }

    private async Task CrawlAsync(Website entryWebsite)
    {
        Queue<Website> toCrawl = new();
        toCrawl.Enqueue(entryWebsite);

        while(toCrawl.Count > 0)
        {
            lock (ExecutionJob)
            {
                if(ExecutionJob.JobStatus == JobStatus.Stopped)
                {
                    return;
                }
            }

            Website website = toCrawl.Dequeue();

            Stopwatch sw = Stopwatch.StartNew();
            string htmlPlain;

            try
            {
                htmlPlain = await websiteProvider.GetStringAsync(website.Url);
            }
            catch { return; }

            HtmlDocument htmlDom = new HtmlDocument();
            htmlDom.LoadHtml(htmlPlain);

            HtmlNodeCollection linkNodes = htmlDom.DocumentNode.SelectNodes("//a[@href]");
            if(linkNodes is not null)
            {
                foreach (HtmlNode linkNode in linkNodes)
                {
                    string link = linkNode.Attributes["href"].Value;

                    if (VisitedUrlToWebsite.TryGetValue(link, out Website? value))
                    {
                        lock(website)
                        {
                            website.OutgoingWebsites.Add(value);
                        }
                    }
                    else
                    {
                        Website outgoingWebsite = new(link);

                        VisitedUrlToWebsite[outgoingWebsite.Url] = outgoingWebsite;

                        lock(website)
                        {
                            website.OutgoingWebsites.Add(outgoingWebsite);
                        }

                        // crawl iff not visited yet and regex matches
                        if (ExecutionJob.WebsiteExecution.Info.Regex.IsMatch(link))
                        {
                            toCrawl.Enqueue(outgoingWebsite);
                        }
                    }
                }
            }

            website.Title = htmlDom.DocumentNode.SelectSingleNode("//title").InnerText;
            website.CrawlTime = sw.Elapsed;
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Executor));
        }

        websiteProvider.Dispose();
        disposed = true; 
    }
}
