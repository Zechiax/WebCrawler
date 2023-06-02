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

    public class InitializedMockWebsiteProvider : MockWebsiteProvider
    {
        public InitializedMockWebsiteProvider()
        {
            brouciHtml =
            @"
                <!DOCTYPE html>
                <html>
                    <title>Brouci</title>
                    <h1>Brouci</h1>
                    <p>Otravuji pomerne dost <a href=""www.wiki.com/lidi"">lidi</a> a sem tam i <a href=""www.wiki.com/psi"">psi</a>.</p>
                </html>
            ";

            psiHtml =
            @"
                <!DOCTYPE html>
                <html>
                    <title>Psi</title>
                    <h1>Psi</h1>
                    <p>Psi maji radi <a href=""www.wiki.com/lidi"">lidi</a> a boji se <a href=""www.wiki.com/auta"">aut</a>.</p>
                </html>
            ";

            lidiHtml =
            @"
                <!DOCTYPE html>
                <html>
                    <title>Lidi</title>
                    <h1>Lidi</h1>
                    <p><a href=""www.wiki.com/auta""></a></p>
                </html>
            ";

            autaHtml =
            @"
                <!DOCTYPE html>
                <html>
                    <title>Auta</title>
                    <h1><a href=""www.wiki.com/lidi"">lidi</a></h1>
                    <p></p>
                </html>
            ";

            GetStringDelay = TimeSpan.FromMilliseconds(600);

            Init();
        }
    }
}
