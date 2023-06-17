using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Models;
using WebCrawler.Test.ServicesTests.Helpers;

namespace WebCrawler.Test.ExecutorTests;

public class ExecutorTests
{
    private readonly string n = Environment.NewLine;

    [Test]
    public void BoundingRegexTest()
    {

        string expected =
            $"(:http://www.fiki.com/lidi) -> " +
            $"{n}(Auta:http://www.wiki.com/auta) -> (:http://www.fiki.com/lidi)" +
            $"{n}(Brouci:http://www.wiki.com/brouci) -> (:http://www.fiki.com/lidi), (Psi:http://www.wiki.com/psi)" +
            $"{n}(Psi:http://www.wiki.com/psi) -> (:http://www.fiki.com/lidi), (Auta:http://www.wiki.com/auta)" +
            $"{n}";

        MockWebsiteProvider mockWebsiteProvider = new MockWebsiteProvider();

        #region htmls
        mockWebsiteProvider.brouciHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Brouci</title>
                    <h1>Brouci</h1>
                    <p>Otravuji pomerne dost <a href=""http://www.fiki.com/lidi"">lidi</a> a sem tam i <a href=""http://www.wiki.com/psi"">psi</a>.</p>
                </html>
            ";

        mockWebsiteProvider.psiHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Psi</title>
                    <h1>Psi</h1>
                    <p>Psi maji radi <a href=""http://www.fiki.com/lidi"">lidi</a> a boji se <a href=""http://www.wiki.com/auta"">aut</a>.</p>
                </html>
            ";

        mockWebsiteProvider.lidiHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Lidi</title>
                    <h1>Lidi</h1>
                    <p><a href=""http://www.wiki.com/auta""></a></p>
                </html>
            ";

        mockWebsiteProvider.autaHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Auta</title>
                    <h1><a href=""http://www.fiki.com/lidi"">lidi</a></h1>
                    <p></p>
                </html>
            ";
        mockWebsiteProvider.Init();
        #endregion

        Executor executor = new(new WebsiteExecutionJob(new CrawlInfo("http://www.wiki.com/brouci", "http://www.wiki.com/*", TimeSpan.Zero), 0), mockWebsiteProvider);


        executor.StartCrawlAsync().Wait();


        string resultingGraph = executor.ExecutionJob.WebsiteExecution.WebsiteGraph!.GetSnapshot().GetStringRepresentation();

        Assert.That(resultingGraph, Is.EqualTo(expected));
    }

    [Test]
    public void GeneralGraphTest()
    {
        string expected = "(Auta:http://www.wiki.com/auta) -> (Lidi:http://www.wiki.com/lidi)" + Environment.NewLine +
            "(Brouci:http://www.wiki.com/brouci) -> (Lidi:http://www.wiki.com/lidi), (Psi:http://www.wiki.com/psi)" + Environment.NewLine +
            "(Lidi:http://www.wiki.com/lidi) -> (Auta:http://www.wiki.com/auta)" + Environment.NewLine +
            "(Psi:http://www.wiki.com/psi) -> (Auta:http://www.wiki.com/auta), (Lidi:http://www.wiki.com/lidi)" + Environment.NewLine;

        MockWebsiteProvider mockWebsiteProvider = new MockWebsiteProvider();

        #region htmls
        mockWebsiteProvider.brouciHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Brouci</title>
                    <h1>Brouci</h1>
                    <p>Otravuji pomerne dost <a href=""http://www.wiki.com/lidi"">lidi</a> a sem tam i <a href=""http://www.wiki.com/psi"">psi</a>.</p>
                </html>
            ";

        mockWebsiteProvider.psiHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Psi</title>
                    <h1>Psi</h1>
                    <p>Psi maji radi <a href=""http://www.wiki.com/lidi"">lidi</a> a boji se <a href=""http://www.wiki.com/auta"">aut</a>.</p>
                </html>
            ";

        mockWebsiteProvider.lidiHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Lidi</title>
                    <h1>Lidi</h1>
                    <p><a href=""http://www.wiki.com/auta""></a></p>
                </html>
            ";

        mockWebsiteProvider.autaHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Auta</title>
                    <h1><a href=""http://www.wiki.com/lidi"">lidi</a></h1>
                    <p></p>
                </html>
            ";
        mockWebsiteProvider.Init();
        #endregion

        Executor executor = new(new WebsiteExecutionJob(new CrawlInfo("http://www.wiki.com/brouci", "http://www.wiki.com/*", TimeSpan.Zero), 0), mockWebsiteProvider);


        executor.StartCrawlAsync().Wait();

        string resultingGraph = executor.ExecutionJob.WebsiteExecution.WebsiteGraph!.GetSnapshot().GetStringRepresentation();

        Assert.That(resultingGraph, Is.EqualTo(expected));
    }
}
