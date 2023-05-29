using WebCrawler.Models;
using WebCrawler.Test.ExecutorTests;

namespace WebCrawler.Test.ServicesTests;

public class ExecutionManagerTests
{
    [Test]
    public void MockRun()
    {
        MockWebsiteProvider mockWebsiteProvider = new();
        string expected = "(Auta:www.wiki.com/auta) -> (Lidi:www.wiki.com/lidi)" + Environment.NewLine +
            "(Brouci:www.wiki.com/brouci) -> (Lidi:www.wiki.com/lidi), (Psi:www.wiki.com/psi)" + Environment.NewLine +
            "(Lidi:www.wiki.com/lidi) -> (Auta:www.wiki.com/auta)" + Environment.NewLine +
            "(Psi:www.wiki.com/psi) -> (Auta:www.wiki.com/auta), (Lidi:www.wiki.com/lidi)" + Environment.NewLine;

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
        ExecutionManager manager = new(new ExecutionManagerConfiguration() { CrawlConsumersCount = 5 });

        for(int i = 0; i < executorsCount; ++i)
        {
            executors.Add(new Executor("www.wiki.com/brouci", "www.wiki.com/*", TimeSpan.Zero, mockWebsiteProvider));
        }

        for(int i = 0; i < executorsCount; ++i )
        {
            manager.AddToQueue(executors[i]);
        }

        manager.WaitForAllConsumersToFinish();

        for(int i = 0; i < executorsCount; ++i)
        {
            string actual = executors[i].WebsiteExecution.GetWebsiteGraph().GetAdjacencyListGraphRepresentation().GetStringRepresentation();
            Assert.That(actual, Is.EqualTo(expected), $"{i}/{executorsCount}");
        }
    }

    [Test]
    public void CheckThatManagerIsUnusableAfterWait()
    {
        const int executorsCount = 10;

        List<Executor> executors = new();
        ExecutionManager manager = new(new ExecutionManagerConfiguration() { CrawlConsumersCount = 5 });

        for(int i = 0; i < executorsCount; ++i)
        {
            executors.Add(new Executor("www.wiki.com/brouci", "www.wiki.com/*", TimeSpan.Zero, new MockWebsiteProvider()));
        }

        for(int i = 0; i < executorsCount; ++i )
        {
            manager.AddToQueue(executors[i]);
        }

        manager.WaitForAllConsumersToFinish();


        Assert.Throws<InvalidOperationException>(() => manager.AddToQueue(executors[0]));
    }
}
