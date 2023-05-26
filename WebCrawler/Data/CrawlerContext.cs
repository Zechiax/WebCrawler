using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

namespace WebCrawler.Data;

public class CrawlerContext : DbContext
{
    public DbSet<WebsiteRecord> WebsiteRecords { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<Execution> Executions { get; set; } = null!;
    
    public CrawlerContext(DbContextOptions<CrawlerContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}