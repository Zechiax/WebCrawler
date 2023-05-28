using WebCrawler.Interfaces;

namespace WebCrawler.Test.ExecutorTests
{
    public class MockWebsiteProvider : IWebsiteProvider
    {
        public string brouciHtml { get; set; } = null!;
        public string psiHtml { get; set; } = null!;
        public string lidiHtml { get; set; } = null!;
        public string autaHtml { get; set; } = null!;

        public TimeSpan GetStringDelay { get; set; }

        private Dictionary<string, string> urlToHtml = new(); 

        public void Init()
        {
            urlToHtml = new()
            {
                { "www.wiki.com/brouci", brouciHtml },
                { "www.wiki.com/psi", psiHtml },
                { "www.wiki.com/lidi", lidiHtml },
                { "www.wiki.com/auta", autaHtml },
            };
        }

        public void Dispose() { } 

        public Task<string> GetStringAsync(string url)
        {
            Thread.Sleep(GetStringDelay);
            return Task.FromResult(urlToHtml[url]);
        }
    }
}
