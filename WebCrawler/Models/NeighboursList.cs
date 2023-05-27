using System.Text;

namespace WebCrawler.Models;

public readonly struct NeighboursList
{
    private readonly Dictionary<Website, List<Website>> neighboursListData;

    public NeighboursList(Dictionary<Website, List<Website>> neighboursListData)
    {
        this.neighboursListData = neighboursListData;
    }

    public string GetStringRepresentation()
    {
        StringBuilder strRepresentation = new("");

        List<Website> vertices = neighboursListData.Keys.ToList();
        vertices.Sort((w1, w2) => w1.Url.CompareTo(w2.Url));
        foreach(Website website in vertices)
        {
            strRepresentation.Append($"({website.ToString()}) -> ");
            List<Website> neighbours = neighboursListData[website];

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

