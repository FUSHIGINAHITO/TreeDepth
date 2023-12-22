using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    private List<ComparableGraph> comparableGraphs = new();

    private float radius = 2f;
    private System.Random random = new();

    private class ComparableGraph : IComparable<ComparableGraph>
    {
        public Graph<Node, Link> graph;
        public float center = 0;

        public ComparableGraph(Graph<Node, Link> _graph)
        {
            graph = _graph;

            if (graph.VertexNum > 0)
            {
                foreach (var node in graph.Vertices)
                {
                    center += node.Value.value.transform.localPosition.x;
                }

                center /= graph.VertexNum;
            }
        }

        public int CompareTo(ComparableGraph other)
        {
            if (center < other.center)
            {
                return -1;
            }

            if (center > other.center)
            {
                return 1;
            }

            else return 0;
        }
    }

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
            Shuffle(num, shuffle);

            var c = random.Next(7);
            foreach (var v in shuffle)
            {
                if (c > 0)
                {
                    rowData.Add(v);
                    c--;
                }
                else
                {
                    break;
                }
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
            (res[j], res[i]) = (res[i], res[j]);
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
                e.Value.value.Hide(node);
            }
            node.Remove();

            graph = node.graphPanel.graph;
            graphPanel = node.graphPanel;
            graphPanel.AddStep(1);
            UpdateStep();
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
            comparableGraphs.Clear();
            foreach (var newGraph in newGraphs)
            {
                comparableGraphs.Add(new(newGraph));
            }
            comparableGraphs.Sort();

            graphPool.Return(graph);
            var remainedGraph = comparableGraphs[0];

            foreach (var comparableGraph in comparableGraphs)
            {
                if (comparableGraph != remainedGraph)
                {
                    CreateGraphPanel(graphPanel, comparableGraph.graph, graphPanel.step);
                }
                else
                {
                    graphPanel.SetGraph(remainedGraph.graph);
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

                if (graphPanel.step + graphPanel.graph.VertexNum <= step)
                {
                    if (graphPanel.step < step)
                    {
                        graphPanel.SetColor(MyColor.gray, MyColor.gray);
                    }
                    else
                    {
                        graphPanel.SetColor(MyColor.gray, MyColor.red);
                    }
                }
                else if (graphPanel.step < step)
                {
                    graphPanel.SetColor(MyColor.green, MyColor.green);
                }
                else
                {
                    graphPanel.SetColor(MyColor.orange, MyColor.red);
                }

                count++;
            }
        }

        levelText.text = "Level " + levelPtr.ToString();
        levelText.color = MyColor.cyan;
        stepText.text = step.ToString();
        stepText.color = MyColor.red;
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
            var node = nodeObj.GetComponent<Node>();
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

                    linkObj.transform.rotation = Quaternion.AngleAxis(Vector2.SignedAngle(vj.Value.value.relPos - vi.Value.value.relPos, -Vector2.right), -Vector3.forward);
                    linkObj.transform.position = 0.5f * (vi.Value.value.relPos + vj.Value.value.relPos);

                    var link = linkObj.GetComponent<Link>();
                    link.targetScale = new Vector3((vi.Value.value.relPos - vj.Value.value.relPos).magnitude, 1, 1);
                    link.Init(vi.Value.value, vj.Value.value, linkObj);
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

    public void AutoDelete()
    {
        foreach (var graphPanel in graphPanels)
        {
            if (graphPanel.step + graphPanel.graph.VertexNum <= step)
            {
                graphPanel.Hide();
                graphPool.Return(graphPanel.graph);
                graphPanels.Remove(graphPanel.node);

                UpdateUI();
                return;
            }
        }
    }
}
