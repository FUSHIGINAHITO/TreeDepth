using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject squarePrefab;
    public GameObject nodePrefab;
    public GameObject linkPrefab;
    public GameObject graphPanelPrefab;

    public TMP_Text levelText;
    public TMP_Text stepText;

    public float H = 50;
    public float W = 70;
    public List<Vector2> res = new();

    private int levelPtr = 0;
    private List<List<int>> data = new();
    public LinkedList<GraphPanel> graphPanels = new();
    private int size;

    private GraphPool<Node, Link> graphPool = new();
    private List<Graph<Node, Link>> newGraphs = new();

    private float radius = 2f;
    private System.Random random = new();

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

        var num = random.Next(1, 20);
        data.Clear();
        for (int i = 0; i < num; i++)
        {
            List<int> rowData = new();
            data.Add(rowData);

            List<int> shuffle = new();
            var c = random.Next(num);
            Shuffle(c, shuffle);

            foreach (var v in shuffle)
            {
                rowData.Add(v); 
            }
        }

        size = data.Count;

        return true;

        //return false;
    }

    public void Shuffle(int k, List<int> res)
    {
        //Fisher–Yates Shuffle
        res.Clear();
        for (int i = 0; i < k; i++)
        {
            res.Add(i);
        }

        for (int i = k - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            int temp = res[i];
            res[i] = res[j];
            res[j] = temp;
        }
    }

    public void RemoveNode(Node node = null)
    {
        Graph<Node, Link> graph;
        GraphPanel graphPanel;

        if (node != null)
        {
            foreach (var e in node.vertex.Neighbors)
            {
                e.Value.value.Hide();
            }
            node.Remove();

            graph = node.graphPanel.graph;
            graphPanel = node.graphPanel;
            graphPanel.AddStep(1);
            UpdateStep();

            if (graphPanel.graph.VertexNum == 0)
            {
                graphPanel.Hide();
                graphPool.Return(graph);
                graphPanels.Remove(graphPanel.node);

                UpdateUI();
                return;
            }
        }
        else
        {
            graphPanel = graphPanels.First.Value;
            graph = graphPanel.graph;
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
                    CreateGraphPanel(graphPanel, newGraph, graphPanel.step);
                }
                else
                {
                    graphPanel.SetGraph(remainedGraph);
                }
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (graphPanels.Count > 0)
        {
            GetLayout(graphPanels.Count, W, H, res, out float a);

            int count = 0;
            foreach (var graphPanel in graphPanels)
            {
                graphPanel.targetPos = res[count];
                graphPanel.targetScale = a * Vector3.one;

                count++;
            }
        }

        levelText.text = "Level " + levelPtr.ToString();
        stepText.text = step.ToString();
    }

    public void Create()
    {
        step = 0;
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

        var graphPanel = CreateGraphPanel(null, graph, 0);

        RemoveNode();

        UpdateUI();
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

    public int step = 0;

    public void UpdateStep()
    {
        foreach (var graphPanel in graphPanels)
        {
            if (graphPanel.step > step)
            {
                step = graphPanel.step;
            }
        }
    }

    public GraphPanel CreateGraphPanel(GraphPanel oldPanel, Graph<Node, Link> graph, int step)
    {
        GameObject graphPanelRoot = Instantiate(graphPanelPrefab);
        graphPanelRoot.transform.parent = transform;
        var graphPanel = graphPanelRoot.GetComponent<GraphPanel>();

        if (oldPanel != null)
        {
            graphPanelRoot.transform.position = oldPanel.transform.position;
            graphPanelRoot.transform.localScale = oldPanel.transform.localScale;
        }
        graphPanel.SetSize(2 * radius);

        graphPanel.Init(graph, step);

        if (oldPanel == null)
        {
            graphPanel.node = graphPanels.AddLast(graphPanel);
        }
        else
        {
            graphPanel.node = graphPanels.AddAfter(oldPanel.node, graphPanel);
        }

        return graphPanel;
    }

    private void GetLayout(int n, float W, float H, List<Vector2> res, out float a)
    {
        res.Clear();

        int r = 0;
        int c = 0;
        a = 1;

        while (r * c < n)
        {
            c++;
            if (W * r < c * H)
            {
                a = W / c;
                if (a * (r + 1) < H)
                {
                    r++;
                    a = H / r;
                    c = Mathf.FloorToInt(W / a);
                }
            }
            else
            {
                a = H / r;
            }
        }

        Vector2 offset = new(0.5f * (c * a - a), 0.5f * (r * a - a));
        //Vector2 offset = new(0.5f * (W - W / c), 0.5f * (H - H / r));
        int remain = n % r;
        for (int i = r - 1; i >= 0; i--)
        {
            int cn = n / r;
            if (remain > 0)
            {
                cn++;
                remain--;
            }

            float rowOffset = 0.5f * (c - cn) * a;

            for (int j = 0; j < cn; j++)
            {
                Vector2 pos = new(j * a + rowOffset, i * a);
                //Vector2 pos = new(j * W / c + rowOffset, i * H / r);

                res.Add(pos - offset);
            }
        }
    }
}
