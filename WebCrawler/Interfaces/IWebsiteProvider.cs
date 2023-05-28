namespace WebCrawler.Interfaces
{
    public interface IWebsiteProvider : IDisposable
    {
        Task<string> GetStringAsync(string url);
    }
}
