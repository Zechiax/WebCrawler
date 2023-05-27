using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WebCrawler.Interfaces;

namespace WebCrawler.Models;

public class Executor : ExecutorData, IExecutor, IDisposable
{
    public string EntryUrl { get; init; }
    public Regex Regex { get; init; }
    public TimeSpan Periodicity { get; init; }


    private readonly HttpClient httpClient = new();

    private bool disposed = false;

    public Executor(string entryUrl, string regex, TimeSpan periodicity)
    {
        EntryUrl = entryUrl;
        Regex = new Regex(regex); 
        Periodicity = periodicity;
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
        await CrawlAsync(new Website { Url = EntryUrl });
    }

    private async Task CrawlAsync(Website website)
    {
        Stopwatch sw = Stopwatch.StartNew();
        string htmlPlain;

        try
        {
            htmlPlain = await httpClient.GetStringAsync(website.Url);
        }
        catch { return; }

        HtmlDocument htmlDom = new HtmlDocument();
        htmlDom.LoadHtml(htmlPlain);

        foreach (HtmlNode link in htmlDom.DocumentNode.SelectNodes("//a[@href]"))
        {
            HtmlAttribute att = link.Attributes["href"];
            website.OutgoingWebsites.Add(new Website
            {
                Url = att.Value
            });
        }

        website.CrawlTime = sw.Elapsed;
        website.Title = htmlDom.DocumentNode.SelectSingleNode("//title").InnerText;

        foreach(Website outgoingWebsite in website.OutgoingWebsites)
        {
            await CrawlAsync(outgoingWebsite);
        }
    }

    public void Dispose()
    {
        httpClient.Dispose();
        disposed = true; 
    }
}
