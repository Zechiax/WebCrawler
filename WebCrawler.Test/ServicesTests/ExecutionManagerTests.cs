using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using WebCrawler.Interfaces;
using WebCrawler.Models;
using WebCrawler.Services;
using WebCrawler.Test.ServicesTests.Helpers;

namespace WebCrawler.Test.ServicesTests;

public class ExecutionManagerTests
{
    private IServiceProvider _serviceProvider = null!;
    
    [SetUp]
    public void SetUp()
    {
        var serviceCollection = new ServiceCollection();
        
        var moqLoggerExecutionManager = new Mock<ILogger<ExecutionManagerService>>();
        var moqLoggerCrawler = new Mock<ILogger<Crawler>>();
        var moqIDataService = new Mock<IDataService>();

        serviceCollection.AddSingleton(moqLoggerExecutionManager.Object);
        serviceCollection.AddSingleton(moqLoggerCrawler.Object);
        serviceCollection.AddSingleton<IDataService>(moqIDataService.Object);
        
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Test]
    public void MockRunTest()
    {
        string expected = "(Auta:http://www.wiki.com/auta) -> (Lidi:http://www.wiki.com/lidi)" + Environment.NewLine +
            "(Brouci:http://www.wiki.com/brouci) -> (Lidi:http://www.wiki.com/lidi), (Psi:http://www.wiki.com/psi)" + Environment.NewLine +
            "(Lidi:http://www.wiki.com/lidi) -> (Auta:http://www.wiki.com/auta)" + Environment.NewLine +
            "(Psi:http://www.wiki.com/psi) -> (Auta:http://www.wiki.com/auta), (Lidi:http://www.wiki.com/lidi)" + Environment.NewLine;

        const int jobsCount = 5;

        List<CrawlInfo> toCrawl = new();

        ExecutionManagerService manager = new(_serviceProvider, new ExecutionManagerConfig()
        {
            CrawlersCount = 10,
            TWebsiteProvider = typeof(InitializedMockWebsiteProvider)
        });

        for(int i = 0; i < jobsCount; ++i)
        {
            toCrawl.Add(new CrawlInfo("http://www.wiki.com/brouci", "http://www.wiki.com/*", TimeSpan.Zero));
        }

        var jobIds = Enumerable.Range(0, jobsCount).ToList();
        foreach (int i in jobIds)
        {
            manager.EnqueueForCrawl(toCrawl[i], (ulong)i);
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
        string expected = "(Auta:http://www.wiki.com/auta) -> (Lidi:http://www.wiki.com/lidi)" + Environment.NewLine +
            "(Brouci:http://www.wiki.com/brouci) -> (Lidi:http://www.wiki.com/lidi), (Psi:http://www.wiki.com/psi)" + Environment.NewLine +
            "(Lidi:http://www.wiki.com/lidi) -> (Auta:http://www.wiki.com/auta)" + Environment.NewLine +
            "(Psi:http://www.wiki.com/psi) -> (Auta:http://www.wiki.com/auta), (Lidi:http://www.wiki.com/lidi)" + Environment.NewLine;

        // jobsCount should be big enough to be able to stop the last job while the crawlers still not dequeid it
        const int jobsCount = 60;

        List<CrawlInfo> toCrawl = new();

        ExecutionManagerService manager = new(_serviceProvider, new ExecutionManagerConfig()
        {
            CrawlersCount = 3,
            TWebsiteProvider = typeof(InitializedMockWebsiteProvider)
        });

        for (int i = 0; i < jobsCount; ++i)
        {
            toCrawl.Add(new CrawlInfo("http://www.wiki.com/brouci", "http://www.wiki.com/*", TimeSpan.Zero));
        }

        List<int> jobIds = Enumerable.Range(0, jobsCount).ToList();
        for (int i = 0; i < jobsCount; ++i)
        {
            manager.EnqueueForCrawl(toCrawl[i], (ulong)jobIds[i]);
        }

        for (int i = 0; i < jobsCount; ++i)
        {
            ulong jobId = (ulong)jobIds[i];

            bool wasStoppedSuccessfuly = manager.StopExecutionAsync(jobId).Result;
        }

        jobIds.Clear();
        jobIds = Enumerable.Range(0, jobsCount).ToList();
        for (int i = 0; i < jobsCount; ++i)
        {
            manager.EnqueueForCrawl(toCrawl[i], (ulong)jobIds[i]);
        }

        for (int i = 0; i < jobIds.Count; i++)
        {
            ulong jobId = (ulong)jobIds[i];

            string actual = manager.GetFullGraphAsync(jobId).Result.GetStringRepresentation();
            Assert.That(actual, Is.EqualTo(expected), $"{jobId}/{jobsCount}");
        }
    }
}
