using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

namespace WebCrawler.Data;

public class CrawlerContext : DbContext
{
    public DbSet<WebsiteRecord> WebsiteRecords { get; set; } = null!;
    public DbSet<Website> Websites { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ExecutorData> Executions { get; set; } = null!;
    
    public CrawlerContext(DbContextOptions<CrawlerContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}