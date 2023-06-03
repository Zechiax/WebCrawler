using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using WebCrawler.Models;
using WebCrawler.Services;
using WebCrawler.Test.ServicesTests.Helpers;

namespace WebCrawler.Test.ServicesTests;

public class ExecutionManagerTests
{
    [Test]
    public void MockRunTest()
    {
        string expected = "(Auta:www.wiki.com/auta) -> (Lidi:www.wiki.com/lidi)" + Environment.NewLine +
            "(Brouci:www.wiki.com/brouci) -> (Lidi:www.wiki.com/lidi), (Psi:www.wiki.com/psi)" + Environment.NewLine +
            "(Lidi:www.wiki.com/lidi) -> (Auta:www.wiki.com/auta)" + Environment.NewLine +
            "(Psi:www.wiki.com/psi) -> (Auta:www.wiki.com/auta), (Lidi:www.wiki.com/lidi)" + Environment.NewLine;

        const int jobsCount = 100;
        ILogger<ExecutionManagerService> logger = new Mock<ILogger<ExecutionManagerService>>().Object;

        List<CrawlInfo> toCrawl = new();
        ExecutionManagerService manager = new(logger, new ExecutionManagerConfig()
        {
            CrawlersCount = 10,
            TWebsiteProvider = typeof(InitializedMockWebsiteProvider)
        });

        for(int i = 0; i < jobsCount; ++i)
        {
            toCrawl.Add(new CrawlInfo("www.wiki.com/brouci", "www.wiki.com/*", TimeSpan.Zero));
        }


        List<ulong> jobIds = new();
        for(int i = 0; i < jobsCount; ++i )
        {
            jobIds.Add(manager.EnqueueForCrawl(toCrawl[i]));
        }


        foreach(ulong jobId in jobIds)
        {
            string actual = manager.GetFullGraphAsync(jobId).Result.GetStringRepresentation();
            Assert.That(actual, Is.EqualTo(expected), $"{jobId}/{jobsCount}");
        }
    }

    [Test]
    public void StopAllRunAllAgainTest()
    {
        string expected = "(Auta:www.wiki.com/auta) -> (Lidi:www.wiki.com/lidi)" + Environment.NewLine +
            "(Brouci:www.wiki.com/brouci) -> (Lidi:www.wiki.com/lidi), (Psi:www.wiki.com/psi)" + Environment.NewLine +
            "(Lidi:www.wiki.com/lidi) -> (Auta:www.wiki.com/auta)" + Environment.NewLine +
            "(Psi:www.wiki.com/psi) -> (Auta:www.wiki.com/auta), (Lidi:www.wiki.com/lidi)" + Environment.NewLine;

        // jobsCount should be big enough to be able to stop the last job while the crawlers still not dequeid it
        const int jobsCount = 4;

        ILogger<ExecutionManagerService> logger = new Mock<ILogger<ExecutionManagerService>>().Object;

        List<CrawlInfo> toCrawl = new();
        ExecutionManagerService manager = new(logger, new ExecutionManagerConfig()
        {
            CrawlersCount = 3,
            TWebsiteProvider = typeof(InitializedMockWebsiteProvider)
        });

        for (int i = 0; i < jobsCount; ++i)
        {
            toCrawl.Add(new CrawlInfo("www.wiki.com/brouci", "www.wiki.com/*", TimeSpan.Zero));
        }

        List<ulong> jobIds = new();
        for (int i = 0; i < jobsCount; ++i)
        {
            jobIds.Add(manager.EnqueueForCrawl(toCrawl[i]));
        }

        for (int i = 0; i < jobsCount; ++i)
        {
            ulong jobId = jobIds[i];

            bool wasStoppedSuccessfuly = manager.StopExecutionAsync(jobId).Result;
        }

        jobIds.Clear();
        for (int i = 0; i < jobsCount; ++i)
        {
            jobIds.Add(manager.EnqueueForCrawl(toCrawl[i]));
        }

        for (int i = 0; i < jobIds.Count; i++)
        {
            ulong jobId = jobIds[i];

            string actual = manager.GetFullGraphAsync(jobId).Result.GetStringRepresentation();
            Assert.That(actual, Is.EqualTo(expected), $"{jobId}/{jobsCount}");
        }
    }
}
