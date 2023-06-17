using HtmlAgilityPack;
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

    private readonly Queue<Website> toCrawl = new();

    private readonly IWebsiteProvider websiteProvider;
    private Dictionary<string, Website> VisitedUrlToWebsite = new();
    private bool disposed = false;
    private readonly Regex _regex;

    public Executor(WebsiteExecutionJob execution, IWebsiteProvider? websiteProvider = null)
    {
        this.websiteProvider = websiteProvider ?? new WebsiteProvider();

        ExecutionJob = execution;
        ExecutionJob.WebsiteExecution.WebsiteGraph = new WebsiteGraph(new Website(execution.CrawlInfo.EntryUrl));
        
        _regex = new Regex(execution.CrawlInfo.RegexPattern, RegexOptions.Compiled);
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
        await CrawlAsync();
        ExecutionJob.WebsiteExecution.Finished = DateTime.Now;
    }

    private async Task CrawlAsync()
    {
        InitCrawling();

        while (!CrawlFinished())
        {
            await ProcessOne();
        }
    }

    public void InitCrawling()
    {
        toCrawl.Enqueue(ExecutionJob.WebsiteExecution.WebsiteGraph!.EntryWebsite);
    }

    public bool CrawlFinished()
    {
        return toCrawl.Count <= 0;
    }

    public async Task ProcessOne()
    {
        lock (ExecutionJob)
        {
            if (ExecutionJob.JobStatus == JobStatus.Stopped)
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
        if (linkNodes is not null)
        {
            foreach (HtmlNode linkNode in linkNodes)
            {
                string link = linkNode.Attributes["href"].Value;

                if (VisitedUrlToWebsite.TryGetValue(link, out Website? value))
                {
                    lock (website)
                    {
                        website.OutgoingWebsites.Add(value);
                    }
                }
                else
                {
                    Website outgoingWebsite = new(link);

                    VisitedUrlToWebsite[outgoingWebsite.Url] = outgoingWebsite;

                    lock (website)
                    {
                        website.OutgoingWebsites.Add(outgoingWebsite);
                    }

                    // crawl iff not visited yet and regex matches
                    if (_regex.IsMatch(link))
                    {
                        toCrawl.Enqueue(outgoingWebsite);
                    }
                }
            }
        }

        website.Title = htmlDom.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "???";
        website.CrawlTime = sw.Elapsed;
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
