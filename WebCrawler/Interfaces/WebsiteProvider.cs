namespace WebCrawler.Interfaces
{
    public class WebsiteProvider : IWebsiteProvider
    {
        private HttpClient httpClient = new();
        public void Dispose()
        {
            httpClient.Dispose();
        }

        public async Task<string> GetStringAsync(string url)
        {
            return await httpClient.GetStringAsync(url);
        }
    }
}
