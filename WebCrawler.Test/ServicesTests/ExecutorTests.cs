using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Models;
using WebCrawler.Test.ServicesTests.Helpers;

namespace WebCrawler.Test.ExecutorTests
{
    public class ExecutorTests
    {
        private readonly string n = Environment.NewLine;

        [Test]
        public void BoundingRegexTest()
        {

            string expected =
                $"(:www.fiki.com/lidi) -> " +
                $"{n}(Auta:www.wiki.com/auta) -> (:www.fiki.com/lidi)" +
                $"{n}(Brouci:www.wiki.com/brouci) -> (:www.fiki.com/lidi), (Psi:www.wiki.com/psi)" +
                $"{n}(Psi:www.wiki.com/psi) -> (:www.fiki.com/lidi), (Auta:www.wiki.com/auta)" +
                $"{n}";

            MockWebsiteProvider mockWebsiteProvider = new MockWebsiteProvider();

            #region htmls
            mockWebsiteProvider.brouciHtml =
            @"
                <!DOCTYPE html>
                <html>
                    <title>Brouci</title>
                    <h1>Brouci</h1>
                    <p>Otravuji pomerne dost <a href=""www.fiki.com/lidi"">lidi</a> a sem tam i <a href=""www.wiki.com/psi"">psi</a>.</p>
                </html>
            ";

            mockWebsiteProvider.psiHtml =
            @"
                <!DOCTYPE html>
                <html>
                    <title>Psi</title>
                    <h1>Psi</h1>
                    <p>Psi maji radi <a href=""www.fiki.com/lidi"">lidi</a> a boji se <a href=""www.wiki.com/auta"">aut</a>.</p>
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
                    <h1><a href=""www.fiki.com/lidi"">lidi</a></h1>
                    <p></p>
                </html>
            ";
            mockWebsiteProvider.Init();
            #endregion

            Executor executor = new(new CrawlInfo("www.wiki.com/brouci", "www.wiki.com/*", TimeSpan.Zero), mockWebsiteProvider);


            executor.StartCrawlAsync().Wait();


            string resultingGraph = executor.WebsiteExecution.WebsiteGraph.GetSnapshot().GetStringRepresentation();

            Assert.That(resultingGraph, Is.EqualTo(expected));
        }

        [Test]
        public void GeneralGraphTest()
        {
            string expected = "(Auta:www.wiki.com/auta) -> (Lidi:www.wiki.com/lidi)" + Environment.NewLine +
                "(Brouci:www.wiki.com/brouci) -> (Lidi:www.wiki.com/lidi), (Psi:www.wiki.com/psi)" + Environment.NewLine +
                "(Lidi:www.wiki.com/lidi) -> (Auta:www.wiki.com/auta)" + Environment.NewLine +
                "(Psi:www.wiki.com/psi) -> (Auta:www.wiki.com/auta), (Lidi:www.wiki.com/lidi)" + Environment.NewLine;

            MockWebsiteProvider mockWebsiteProvider = new MockWebsiteProvider();

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

            Executor executor = new(new CrawlInfo("www.wiki.com/brouci", "www.wiki.com/*", TimeSpan.Zero), mockWebsiteProvider);


            executor.StartCrawlAsync().Wait();

            string resultingGraph = executor.WebsiteExecution.WebsiteGraph.GetSnapshot().GetStringRepresentation();

            Assert.That(resultingGraph, Is.EqualTo(expected));
        }
    }
}
