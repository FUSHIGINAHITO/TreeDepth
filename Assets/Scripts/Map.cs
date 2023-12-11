using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject linkPrefab;

    private int levelPtr = 0;
    private List<List<int>> data = new();
    public LinkedList<GraphPanel> curMap = new();
    private int size;

    private void Awake()
    {
        LoadData();
    }

    public bool LoadData()
    {
        var txt = Resources.Load<TextAsset>($"Maps/map{levelPtr}");
        if (txt != null)
        {
            data.Clear();
            foreach (var row in txt.text.Split("\r\n"))
            {
                List<int> rowData = new();
                data.Add(rowData);
                foreach (var c in row.Split("\t"))
                {
                    rowData.Add(int.Parse(c));
                }
            }

            size = data.Count;

            return true;
        }

        return false;
    }

    public void Create()
    {
        foreach (var item in curMap)
        {
            Destroy(item.gameObject);
        }

        curMap.Clear();

        Graph<Node> graph = new();

        GameObject graphPanelRoot = new("GraphPanel");
        var graphPanel = graphPanelRoot.AddComponent<GraphPanel>();
        graphPanel.Init(graph, 0);
        curMap.AddLast(graphPanel);

        var radius = 2f;
        for (int i = 0; i < size; i++)
        {
            var nodeObj = Instantiate(nodePrefab);
            nodeObj.transform.parent = graphPanel.transform;

            var theta = 2 * Mathf.PI / size * i;
            Vector2 relPos = radius * new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));
            nodeObj.transform.localPosition = relPos;
            var node = nodeObj.AddComponent<Node>();
            node.Init(nodeObj, relPos);
            graph.AddVertex(node.vertex);
        }


        var ii = 0;
        foreach (var vi in graph.Vertices)
        {
            foreach (var j in data[ii])
            {
                var vj = graph.GetAtIndex(j);

                var linkObj = Instantiate(linkPrefab);
                linkObj.transform.parent = graphPanel.transform;

                linkObj.transform.right = vi.Value.value.relPos - vj.Value.value.relPos;
                linkObj.transform.position = 0.5f * (vi.Value.value.relPos + vj.Value.value.relPos);
                linkObj.transform.localScale = new Vector3((vi.Value.value.relPos - vj.Value.value.relPos).magnitude, 1, 1);
                vi.Value.AddNeighbor(vj.Value);
            }
            ii++;
        }
    }

    public void LastLevel()
    {
        levelPtr -= 1;
        if (!LoadData())
        {
            levelPtr += 1;
        }
    }

    public void NextLevel()
    {
        levelPtr += 1;
        if (!LoadData())
        {
            levelPtr -= 1;
        }
    }

    public bool Satisfied
    {
        get
        {
            return curMap.Count == 0;
        }
    }
}
