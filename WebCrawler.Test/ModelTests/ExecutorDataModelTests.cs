using WebCrawler.Models;

namespace WebCrawler.Test.ModelTests;

[TestFixture]
public class ExecutorDataModelTests
{
    private Website WebTree { get; set; } = null!;
    
    private static readonly string[] Urls = new string[]
    {
        "https://www.example.com",
        "https://www.example.com/about",
        "https://www.example.com/contact",
        "https://www.example.com/privacy",
    }; 
    
    private static readonly string[] Titles = new string[]
    {
        "Example Domain",
        "Example Domain - About",
        "Example Domain - Contact",
        "Example Domain - Privacy",
    };
    
    [SetUp]
    public void Setup()
    {
        PopulateWebTree();
    }

    private void PopulateWebTree()
    {
        var website = new Website
        {
            Url = Urls[0],
            Title = Titles[0],
        };
        
        for (var i = 1; i < Urls.Length; i++)
        {
            website.OutgoingWebsites.Add(new Website
            {
                Url = Urls[i],
                Title = Titles[i],
            });
            
            website = website.OutgoingWebsites.First();
        }
        
        WebTree = website;
    }

    [Test]
    public void Execution_WithoutWebTree_ReturnsNull()
    {
        var execution = new ExecutorData();

        var tree = execution.GetWebTree();
        
        Assert.That(tree, Is.Null);
    }

    [Test]
    public void Execution_Serialize_DeserializeWebTree()
    {
        var execution = new ExecutorData();
        
        execution.SetWebTree(WebTree);

        var tree = execution.GetWebTree();
        var currentCheckTree = WebTree;
        
        // We check that the tree is the same as the original tree
        while (tree != null)
        {
            Assert.Multiple(() =>
            {
                Assert.That(WebTree, Is.Not.Null);
                Assert.That(tree.Url, Is.EqualTo(WebTree.Url));
                Assert.That(tree.Title, Is.EqualTo(WebTree.Title));
                Assert.That(tree.OutgoingWebsites, Is.EquivalentTo(WebTree.OutgoingWebsites));
            });
            
            tree = tree.OutgoingWebsites.FirstOrDefault();
            currentCheckTree = currentCheckTree!.OutgoingWebsites.FirstOrDefault();
        }
    }

}