using Microsoft.EntityFrameworkCore;
using WebCrawler.Interfaces;
using WebCrawler.Models;

namespace WebCrawler.Data;

public static class SeedData
{
    public static async Task SeedDataAsync(CrawlerContext context, IDataService dataService)
    {
        // if there are some website records, we end
        if (await context.WebsiteRecords.AnyAsync())
            return;
        
        // otherwise we seed some data
        var websiteRecords = new List<WebsiteRecord>()
        {
            new WebsiteRecord()
            {
                IsActive = true,
                Created = DateTime.Now - TimeSpan.FromDays(1),
                Tags = new List<WebTag>()
                {
                    new WebTag()
                    {
                        Name = "Search engine"
                    },
                    new WebTag()
                    {
                        Name = "Google"
                    }
                },
                Label = "Google",
                CrawlInfo = new()
                {
                    EntryUrl = "https://www.google.com",
                    RegexPattern = ".*",
                    Periodicity = TimeSpan.FromHours(1),
                    LastExecution = new WebsiteExecution()
                    {
                        Started = DateTime.Now - TimeSpan.FromHours(1),
                        Finished = DateTime.Now,
                    }
                }
            },
            new WebsiteRecord()
            {
                Tags = new List<WebTag>()
                {
                    new WebTag()
                    {
                        Name = "Extreamly long tag name to see what happens"
                    },
                    new WebTag()
                    {
                        Name = "Social Media"
                    },
                    new WebTag()
                    {
                        Name = "Facebook"
                    }
                },
                Label = "Facebook",
                CrawlInfo = new()
                {
                    EntryUrl = "https://www.facebook.com",
                    RegexPattern = ".*",
                    Periodicity = TimeSpan.FromHours(2),
                    LastExecution = null
                }
            },
            new WebsiteRecord()
            {
                Tags = new List<WebTag>()
                {
                    new WebTag()
                    {
                        Name = "E-commerce"
                    },
                    new WebTag()
                    {
                        Name = "Amazon"
                    },
                    new WebTag()
                    {
                        Name = "Shopping"
                    },
                    new WebTag()
                    {
                        Name = "Books"
                    },
                    new WebTag()
                    {
                        Name = "Electronics"
                    }
                },
                Created = DateTime.Now - TimeSpan.FromDays(2) - TimeSpan.FromHours(3),
                Label = "Amazon",
                CrawlInfo = new()
                {
                    EntryUrl = "https://www.amazon.com",
                    RegexPattern = ".*",
                    Periodicity = TimeSpan.FromHours(3),
                    LastExecution = new WebsiteExecution()
                    {
                        Started = DateTime.Now - TimeSpan.FromHours(3),
                        Finished = DateTime.Now,
                    }
                }
            },
            new WebsiteRecord()
            {
                IsActive = true,
                Tags = new List<WebTag>()
                {
                    new WebTag()
                    {
                        Name = "Technology News"
                    },
                    new WebTag()
                    {
                        Name = "TechCrunch"
                    }
                },
                Label = "TechCrunch",
                CrawlInfo = new()
                {
                    EntryUrl = "https://techcrunch.com",
                    // some random regex
                    RegexPattern = ".r{1,2}a*ndom",
                    Periodicity = TimeSpan.FromHours(1),
                    LastExecution = null
                }
            },
            new WebsiteRecord()
            {
                Created = DateTime.Now + TimeSpan.FromDays(42),
                Tags = new List<WebTag>()
                {
                    new WebTag()
                    {
                        Name = "Programming"
                    },
                    new WebTag()
                    {
                        Name = "StackOverflow"
                    },
                    new WebTag()
                    {
                        Name = "C#"
                    },
                    new WebTag()
                    {
                        Name = "Java"
                    },
                    new WebTag()
                    {
                        Name = "Python"
                    },
                    new WebTag()
                    {
                        Name = "C++"
                    },
                    new WebTag()
                    {
                        Name = "C"
                    },
                    new WebTag()
                    {
                        Name = "JavaScript"
                    },
                    new WebTag()
                    {
                        Name = "PHP"
                    },
                    new WebTag()
                    {
                        Name = "Go"
                    }
                },
                Label = "StackOverflow",
                CrawlInfo = new()
                {
                    EntryUrl = "https://stackoverflow.com",
                    RegexPattern = ".*",
                    Periodicity = TimeSpan.FromHours(4)
                }
            }
        };

        foreach (var websiteRecord in websiteRecords)
        {
            await dataService.AddWebsiteRecord(websiteRecord);
        }
    }
}