using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject linkPrefab;
    public GameObject graphPanelPrefab;

    private int levelPtr = 0;
    private List<List<int>> data = new();
    public LinkedList<GraphPanel> graphPanels = new();
    private int size;

    private GraphPool<Node, Link> graphPool = new();
    private List<Graph<Node, Link>> newGraphs = new();

    private float radius = 2f;

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
                    if (int.TryParse(c, out int v))
                    {
                        rowData.Add(v);
                    }
                }
            }

            size = data.Count;

            return true;
        }

        return false;
    }

    public void RemoveNode(Node node)
    {
        foreach (var e in node.vertex.Neighbors)
        {
            e.Value.value.Hide();
        }
        node.Remove();

        var graph = node.graphPanel.graph;

        var graphPanel = node.graphPanel;
        graphPanel.AddStep(1);
        if (graphPanel.graph.VertexNum == 0)
        {
            graphPool.Return(graph);
            graphPanels.Remove(graphPanel);
            return;
        }

        newGraphs.Clear();
        graph.ClearTag();
        foreach (var v in graph.Vertices)
        {
            if (!v.Value.visited)
            {
                var newGraph = graphPool.Require();
                newGraphs.Add(newGraph);

                foreach (var u in graph.BfsEnumerator(v.Value))
                {
                    u.visited = true;
                    newGraph.AddVertex(u);
                }
            }
        }

        if (newGraphs.Count > 1)
        {
            graphPool.Return(graph);
            Graph<Node, Link> remainedGraph = newGraphs[0];
            foreach (var newGraph in newGraphs)
            {
                if (newGraph.VertexNum > remainedGraph.VertexNum)
                {
                    remainedGraph = newGraph;
                }
            }

            foreach (var newGraph in newGraphs)
            {
                if (newGraph != remainedGraph)
                {
                    CreateGraphPanel(graphPanel.transform.position, newGraph, graphPanel.step);
                }
                else
                {
                    graphPanel.SetGraph(remainedGraph);
                }
            }
        }
    }

    public void Create()
    {
        foreach (Transform item in transform)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in graphPanels)
        {
            graphPool.Return(item.graph);
        }
        graphPanels.Clear();

        var graph = graphPool.Require();

        for (int i = 0; i < size; i++)
        {
            var nodeObj = Instantiate(nodePrefab);

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

                if (!vi.Value.Adj(vj.Value))
                {
                    var linkObj = Instantiate(linkPrefab);

                    linkObj.transform.right = vi.Value.value.relPos - vj.Value.value.relPos;
                    linkObj.transform.position = 0.5f * (vi.Value.value.relPos + vj.Value.value.relPos);
                    linkObj.transform.localScale = new Vector3((vi.Value.value.relPos - vj.Value.value.relPos).magnitude, 1, 1);

                    var link = linkObj.AddComponent<Link>();
                    link.Init(linkObj);
                    vi.Value.AddNeighbor(vj.Value, link);
                }
            }
            ii++;
        }

        var graphPanel = CreateGraphPanel(Vector3.zero, graph, 0);
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
            return graphPanels.Count == 0;
        }
    }

    public GraphPanel CreateGraphPanel(Vector3 pos, Graph<Node, Link> graph, int step)
    {
        GameObject graphPanelRoot = Instantiate(graphPanelPrefab);
        graphPanelRoot.transform.parent = transform;
        var graphPanel = graphPanelRoot.GetComponent<GraphPanel>();
        graphPanelRoot.transform.position = pos;

        graphPanel.Init(graph, step);
        graphPanel.SetSize(2f * radius);
        graphPanels.AddLast(graphPanel);

        var id = graphPanels.Count - 3;

        graphPanel.targetPos = id * 3 * Vector3.right;

        return graphPanel;
    }
}
