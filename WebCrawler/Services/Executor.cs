using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

/// <summary>
/// Crawls all websites.
/// </summary>
public class Executor : IExecutor, IDisposable
{
    public CrawlInfo CrawlInfo { get; }
    public WebsiteExecution WebsiteExecution { get; }

    private WebsiteGraph _websiteGraph;

    private readonly IWebsiteProvider websiteProvider;

    private Dictionary<string, Website> VisitedUrlToWebsite = new();

    private bool disposed = false;

    public Executor(CrawlInfo crawlInfo, IWebsiteProvider? websiteProvider = null)
    {
        CrawlInfo = crawlInfo;
        this.websiteProvider = websiteProvider ?? new WebsiteProvider();
        
        _websiteGraph = new WebsiteGraph(new Website(CrawlInfo.EntryUrl));
        
        WebsiteExecution = new WebsiteExecution(_websiteGraph);
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
        await StartCrawlAsync(default(CancellationToken));
    }

    public async Task StartCrawlAsync(CancellationToken ct)
    {
        WebsiteExecution.Started = DateTime.Now;
        await CrawlAsync(_websiteGraph.EntryWebsite, ct)
        .ContinueWith(_ =>
        {
            WebsiteExecution.Finished = DateTime.Now;
            WebsiteExecution.SetWebsiteGraph(_websiteGraph);
        });
    }

    private async Task CrawlAsync(Website website, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

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

                if (!CrawlInfo.Regex.IsMatch(link))
                {
                    continue;
                }

                if (VisitedUrlToWebsite.TryGetValue(link, out Website? value))
                {
                    website.OutgoingWebsites.Add(value);
                }
                else
                {
                    website.OutgoingWebsites.Add(new Website(link));
                }
            }
        }

        website.Title = htmlDom.DocumentNode.SelectSingleNode("//title").InnerText;
        website.CrawlTime = sw.Elapsed;

        foreach(Website outgoingWebsite in website.OutgoingWebsites)
        {
            if (!VisitedUrlToWebsite.ContainsKey(outgoingWebsite.Url))
            {
                VisitedUrlToWebsite[outgoingWebsite.Url] = outgoingWebsite;

                await CrawlAsync(outgoingWebsite, ct);
            }
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
