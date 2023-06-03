using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using WebCrawler.Models;
using WebCrawler.Services;
using WebCrawler.Test.ExecutorTests;

namespace WebCrawler.Test.ServicesTests;

public class ExecutionManagerTests
{
    [Test]
    public void MockRun()
    {
        string expected = "(Auta:www.wiki.com/auta) -> (Lidi:www.wiki.com/lidi)" + Environment.NewLine +
            "(Brouci:www.wiki.com/brouci) -> (Lidi:www.wiki.com/lidi), (Psi:www.wiki.com/psi)" + Environment.NewLine +
            "(Lidi:www.wiki.com/lidi) -> (Auta:www.wiki.com/auta)" + Environment.NewLine +
            "(Psi:www.wiki.com/psi) -> (Auta:www.wiki.com/auta), (Lidi:www.wiki.com/lidi)" + Environment.NewLine;

        const int jobsCount = 5;
        ILogger<ExecutionManagerService<InitializedMockWebsiteProvider>> logger = new Mock<ILogger<ExecutionManagerService<InitializedMockWebsiteProvider>>>().Object;


        List<CrawlInfo> toCrawl = new();
        ExecutionManagerService<InitializedMockWebsiteProvider> manager = new(logger, new ExecutionManagerConfig() { CrawlersCount = 2 });

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
            manager.WaitForExecutorToFinish(jobId);
            string actual = manager.GetGraph(jobId).GetStringRepresentation();
            Assert.That(actual, Is.EqualTo(expected), $"{jobId}/{jobsCount}");
        }
    }
}
