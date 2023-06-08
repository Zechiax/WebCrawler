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
                Tags = new List<Tag>()
                {
                    new Tag()
                    {
                        Name = "Search engine"
                    },
                    new Tag()
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
                Tags = new List<Tag>()
                {
                    new Tag()
                    {
                        Name = "Social Media"
                    },
                    new Tag()
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
                Tags = new List<Tag>()
                {
                    new Tag()
                    {
                        Name = "E-commerce"
                    },
                    new Tag()
                    {
                        Name = "Amazon"
                    }
                },
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
                Tags = new List<Tag>()
                {
                    new Tag()
                    {
                        Name = "Technology News"
                    },
                    new Tag()
                    {
                        Name = "TechCrunch"
                    }
                },
                Label = "TechCrunch",
                CrawlInfo = new()
                {
                    EntryUrl = "https://techcrunch.com",
                    RegexPattern = ".*",
                    Periodicity = TimeSpan.FromHours(1),
                    LastExecution = null
                }
            },
            new WebsiteRecord()
            {
                Tags = new List<Tag>()
                {
                    new Tag()
                    {
                        Name = "Programming"
                    },
                    new Tag()
                    {
                        Name = "StackOverflow"
                    }
                },
                Label = "StackOverflow",
                CrawlInfo = new()
                {
                    EntryUrl = "https://stackoverflow.com",
                    RegexPattern = ".*",
                    Periodicity = TimeSpan.FromHours(4),
                    LastExecution = new WebsiteExecution()
                    {
                        Started = DateTime.Now - TimeSpan.FromHours(4),
                        Finished = DateTime.Now,
                    }
                }
            }
        };

        foreach (var websiteRecord in websiteRecords)
        {
            await dataService.AddWebsiteRecord(websiteRecord);
        }
    }
}