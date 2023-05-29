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
    public string EntryUrl { get; init; }
    public Regex Regex { get; init; }
    public TimeSpan Periodicity { get; init; }
    public WebsiteExecution WebsiteExecution { get; }


    private readonly IWebsiteProvider websiteProvider;

    private Dictionary<string, Website> VisitedUrlToWebsite = new();

    private bool disposed = false;

    public Executor(string entryUrl, string regex, TimeSpan periodicity, IWebsiteProvider? websiteProvider = null)
    {
        EntryUrl = entryUrl;
        Regex = new Regex(regex); 
        Periodicity = periodicity;
        this.websiteProvider = websiteProvider ?? new WebsiteProvider();
        WebsiteExecution = new WebsiteExecution(new WebsiteGraph(new Website(entryUrl)));
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
        WebsiteExecution.Started = DateTime.Now;
        await CrawlAsync(WebsiteExecution.GetAdjacencyList().EntryWebsite).ContinueWith(_ => WebsiteExecution.Finished = DateTime.Now);
    }

    private async Task CrawlAsync(Website website)
    {
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

                if (!Regex.IsMatch(link))
                {
                    continue;
                }

                if (VisitedUrlToWebsite.ContainsKey(link))
                {
                    website.OutgoingWebsites.Add(VisitedUrlToWebsite[link]);
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
                // mark as visited
                VisitedUrlToWebsite[outgoingWebsite.Url] = outgoingWebsite;

                await CrawlAsync(outgoingWebsite);
            }
        }
    }

    public void Dispose()
    {
        websiteProvider.Dispose();
        disposed = true; 
    }
}
