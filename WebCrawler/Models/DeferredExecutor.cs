using WebCrawler.Interfaces;

namespace WebCrawler.Models;

class DeferredExecutor : IExecutor
{
    private Executor _executor;
    private TimeSpan _defer;
    public WebsiteExecutionJob ExecutionJob => _executor.ExecutionJob;

    public DeferredExecutor(TimeSpan defer, WebsiteExecutionJob execution, IWebsiteProvider? websiteProvider = null)
    {
        _executor = new Executor(execution, websiteProvider);
        _defer = defer;
    }

    public async Task StartCrawlAsync()
    {
        ExecutionJob.WebsiteExecution.Started = DateTime.Now;

        _executor.InitCrawling();

        while (!_executor.CrawlFinished())
        {
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
