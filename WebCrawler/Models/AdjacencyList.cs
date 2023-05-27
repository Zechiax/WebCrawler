using System.Text;

namespace WebCrawler.Models;

/// <summary>
/// https://en.wikipedia.org/wiki/Adjacency_list
/// </summary>
public readonly struct AdjacencyList
{
    private readonly Dictionary<Website, List<Website>> adjacencyListData;

    public AdjacencyList(Dictionary<Website, List<Website>> adjacencyListData)
    {
        this.adjacencyListData = adjacencyListData;
    }

    /// <summary>
    /// String representation of the underlying adjacency list.
    /// Websites sorted by Url.
    /// </summary>
    /// <returns></returns>
    public string GetStringRepresentation()
    {
        StringBuilder strRepresentation = new("");

        List<Website> vertices = adjacencyListData.Keys.ToList();
        vertices.Sort((w1, w2) => w1.Url.CompareTo(w2.Url));
        foreach(Website website in vertices)
        {
            strRepresentation.Append($"({website.ToString()}) -> ");
            List<Website> neighbours = adjacencyListData[website];

            if(neighbours.Count != 0)
            {
                neighbours.Sort((w1, w2) => w1.Url.CompareTo(w2.Url));

                strRepresentation.Append($"({neighbours[0]})");
                for(int i = 1; i < neighbours.Count; ++i)
                {
                    strRepresentation.Append($", ({neighbours[i]})");
                }
            }

            strRepresentation.AppendLine();
        }

        return strRepresentation.ToString();
    }
}

