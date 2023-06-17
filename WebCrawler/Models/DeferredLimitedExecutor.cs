using WebCrawler.Interfaces;

namespace WebCrawler.Models;

class DeferredLimitedExecutor : IExecutor
{
    private Executor _executor;
    private TimeSpan _defer;
    private readonly int _maxNodesToCrawl;
    public WebsiteExecutionJob ExecutionJob => _executor.ExecutionJob;

    public DeferredLimitedExecutor(TimeSpan defer, int maxNodesToCrawl, WebsiteExecutionJob execution, IWebsiteProvider? websiteProvider = null)
    {
        _executor = new Executor(execution, websiteProvider);
        _defer = defer;
        _maxNodesToCrawl = maxNodesToCrawl;
    }

    public async Task StartCrawlAsync()
    {
        ExecutionJob.WebsiteExecution.Started = DateTime.Now;

        _executor.InitCrawling();

        while (!_executor.CrawlFinished())
        {
            if(ExecutionJob.WebsiteExecution.WebsiteGraph?.GetSnapshot().Data.Count > _maxNodesToCrawl)
            {
                break;
            }

            await Task.Delay(_defer);
            await _executor.ProcessOne();
        }

        ExecutionJob.WebsiteExecution.Finished = DateTime.Now;
    }
    public void Dispose()
    {
        _executor.Dispose();
    }
}
