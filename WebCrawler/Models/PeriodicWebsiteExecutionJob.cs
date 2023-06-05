namespace WebCrawler.Models;

record class PeriodicWebsiteExecutionJob
{
    public int NumberOfCrawls { get; set; }

    private Task task;
    private CancellationTokenSource cancelSource = new();

    private TimeSpan periodicity;

    public PeriodicWebsiteExecutionJob(TimeSpan periodicity, Action job)
    {
        this.periodicity = periodicity;

        task = new Task(() => PeriodicJob(job, cancelSource.Token), cancelSource.Token);
    }

    public void Start()
    {
        task.Start();
    }

    public async Task<bool> Stop()
    {
        cancelSource.Cancel();

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            return true;
        }

        return false;
    }

    private async void PeriodicJob(Action job, CancellationToken ct)
    {
        using PeriodicTimer timer = new(periodicity);

        while (!ct.IsCancellationRequested && await timer.WaitForNextTickAsync(ct))
        {
            job.Invoke();
        }

        ct.ThrowIfCancellationRequested();
    }
}
