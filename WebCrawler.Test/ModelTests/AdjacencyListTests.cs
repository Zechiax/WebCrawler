using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Models;

namespace WebCrawler.Test.ModelTests
{
    public class AdjacencyListTests
    {
        private string json;
        private WebsiteGraphSnapshot adjacencyList;

        [SetUp]
        public void Setup()
        {
            Website web1 = new("www.web1.com");
            Website web2 = new("www.web2.com");
            Website web3 = new("www.web3.com");
            Website web4 = new("www.web4.com");
            Website web5 = new("www.web5.com");
            Website web6 = new("www.web6.com");
            Website web7 = new("www.web7.com");

            adjacencyList = new(new Dictionary<Website, List<Website>>
            {
                { web1, new List<Website> { web2, web3 } },
                { web2, new List<Website> { web4 } },
                { web3, new List<Website> { web5, web6 } },
                { web4, new List<Website> { web4 } },
                { web5, new List<Website> { web1, web3 } },
                { web6, new List<Website> { web1, web2, web3, web4, web5 } },
                { web7, new List<Website> { } },
            }, web1);

            json = "{\"EntryUrl\":\"www.web1.com\",\"Graph\":[{\"Url\":\"www.web1.com\",\"Title\":\"\",\"CrawlTime\":\"00:00:00\",\"Neighbours\":[\"www.web2.com\",\"www.web3.com\"]},{\"Url\":\"www.web2.com\",\"Title\":\"\",\"CrawlTime\":\"00:00:00\",\"Neighbours\":[\"www.web4.com\"]},{\"Url\":\"www.web3.com\",\"Title\":\"\",\"CrawlTime\":\"00:00:00\",\"Neighbours\":[\"www.web5.com\",\"www.web6.com\"]},{\"Url\":\"www.web4.com\",\"Title\":\"\",\"CrawlTime\":\"00:00:00\",\"Neighbours\":[\"www.web4.com\"]},{\"Url\":\"www.web5.com\",\"Title\":\"\",\"CrawlTime\":\"00:00:00\",\"Neighbours\":[\"www.web1.com\",\"www.web3.com\"]},{\"Url\":\"www.web6.com\",\"Title\":\"\",\"CrawlTime\":\"00:00:00\",\"Neighbours\":[\"www.web1.com\",\"www.web2.com\",\"www.web3.com\",\"www.web4.com\",\"www.web5.com\"]},{\"Url\":\"www.web7.com\",\"Title\":\"\",\"CrawlTime\":\"00:00:00\",\"Neighbours\":[]}]}";
        }

        [Test]
        public void SerializationTest()
        {
            string actual = WebsiteGraphSnapshot.JsonConverter.Serialize(adjacencyList);
            
            Assert.That(actual, Is.EqualTo(json));
        }

        [Test]
        public void DeserializationTest()
        {
            WebsiteGraphSnapshot adjacencyList = WebsiteGraphSnapshot.JsonConverter.Deserialize(json);


            List<Website> actualAllWebsites = adjacencyList.Data.Select(pair => pair.Key).ToList();
            List<Website> expectedAllWebsites = this.adjacencyList.Data.Select(pair => pair.Key).ToList();

            CollectionAssert.AreEqual(expectedAllWebsites.Select(website => website.Url), actualAllWebsites.Select(website => website.Url));
            CollectionAssert.AreEqual(expectedAllWebsites.Select(website => website.Title), actualAllWebsites.Select(website => website.Title));
            CollectionAssert.AreEqual(expectedAllWebsites.Select(website => website.CrawlTime), actualAllWebsites.Select(website => website.CrawlTime));
            
            Assert.That(this.adjacencyList.EntryWebsite?.Url, Is.EqualTo(adjacencyList.EntryWebsite?.Url));

            for(int i = 0; i < actualAllWebsites.Count; ++i)
            {
                CollectionAssert.AreEqual(this.adjacencyList.Data[expectedAllWebsites[i]].Select(website => website.Url),
                    adjacencyList.Data[actualAllWebsites[i]].Select(website => website.Url));

                CollectionAssert.AreEqual(this.adjacencyList.Data[expectedAllWebsites[i]].Select(website => website.Title),
                    adjacencyList.Data[actualAllWebsites[i]].Select(website => website.Title));

                CollectionAssert.AreEqual(this.adjacencyList.Data[expectedAllWebsites[i]].Select(website => website.CrawlTime),
                    adjacencyList.Data[actualAllWebsites[i]].Select(website => website.CrawlTime));
            }
        }
    }
}
