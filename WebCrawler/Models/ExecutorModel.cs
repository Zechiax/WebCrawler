namespace WebCrawler.Models;

/// <summary>
/// Database object.
/// </summary>
public abstract class ExecutorModel
{
    public WebsiteRecordModel EntryWebsite { get; init; }

    public ExecutorModel(WebsiteRecordModel entryWebsite)
    {
        EntryWebsite = entryWebsite;
    }
}
