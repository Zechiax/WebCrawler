﻿using WebCrawler.Models;
using WebCrawler.Test.ExecutorTests;

namespace WebCrawler.Test.ServicesTests;

public class ExecutionManagerTests
{
    [Test]
    public void MockRun()
    {
        MockWebsiteProvider mockWebsiteProvider = new();
        string expected = "(Auta:www.wiki.com/auta) -> (Lidi:www.wiki.com/lidi)\r\n(Brouci:www.wiki.com/brouci) -> (Lidi:www.wiki.com/lidi), (Psi:www.wiki.com/psi)\r\n(Lidi:www.wiki.com/lidi) -> (Auta:www.wiki.com/auta)\r\n(Psi:www.wiki.com/psi) -> (Auta:www.wiki.com/auta), (Lidi:www.wiki.com/lidi)\r\n";

        #region htmls
        mockWebsiteProvider.brouciHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Brouci</title>
                    <h1>Brouci</h1>
                    <p>Otravuji pomerne dost <a href=""www.wiki.com/lidi"">lidi</a> a sem tam i <a href=""www.wiki.com/psi"">psi</a>.</p>
                </html>
            ";

        mockWebsiteProvider.psiHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Psi</title>
                    <h1>Psi</h1>
                    <p>Psi maji radi <a href=""www.wiki.com/lidi"">lidi</a> a boji se <a href=""www.wiki.com/auta"">aut</a>.</p>
                </html>
            ";

        mockWebsiteProvider.lidiHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Lidi</title>
                    <h1>Lidi</h1>
                    <p><a href=""www.wiki.com/auta""></a></p>
                </html>
            ";

        mockWebsiteProvider.autaHtml =
        @"
                <!DOCTYPE html>
                <html>
                    <title>Auta</title>
                    <h1><a href=""www.wiki.com/lidi"">lidi</a></h1>
                    <p></p>
                </html>
            ";
        mockWebsiteProvider.Init();
        #endregion

        mockWebsiteProvider.GetStringDelay = TimeSpan.FromMilliseconds(200);
        const int executorsCount = 10;

        List<Executor> executors = new();
        using (ExecutionManager manager = new(new ExecutionManagerConfiguration() { CrawlConsumersCount = 5 }))
        {
            for(int i = 0; i < executorsCount; ++i)
            {
                executors.Add(new Executor("www.wiki.com/brouci", "www.wiki.com/*", TimeSpan.Zero, mockWebsiteProvider));
            }

            for(int i = 0; i < executorsCount; ++i )
            {
                manager.AddToQueue(executors[i]);
            }
        }

        Thread.Sleep(TimeSpan.FromSeconds(2));

        for(int i = 0; i < executorsCount; ++i)
        {
            string actual = executors[i].WebsiteExecution.WebsiteGraph.GetNeighboursListGraphRepresentation().GetStringRepresentation();
            Assert.That(actual, Is.EqualTo(expected), $"{i}/{executorsCount}");
        }
    }
}
