using System;
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
    public TMP_Text scoreText;

    public float H = 50;
    public float W = 70;
    public List<Vector2> res = new();

    private int levelPtr = 0;
    private int levelNum = 100;

    private class LevelData
    {
        public List<List<int>> graph = new();
        public List<Vector3> pos = new();
        public List<Color> colors = new();
        public int size = -1;
        public string name;

        public void Clear()
        {
            graph.Clear();
            pos.Clear();
            colors.Clear();
            name = "Zen";
            size = -1;
        }
    }

    private LevelData data = new();

    public LinkedList<GraphPanel> graphPanels = new();
    private int score;
    public int step;

    private GraphPool<Node, Link> graphPool = new();
    private List<Graph<Node, Link>> newGraphs = new();
    private List<ComparableGraph> comparableGraphs = new();
    private List<int> shuffle = new();

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

            else
                return 0;
        }
    }

    private void Awake()
    {
        LoadData();
    }

    public void LoadData()
    {
        data.Clear();

        var txt = Resources.Load<TextAsset>($"Maps/map{levelPtr}");
        if (txt != null)
        {
            foreach (var row in txt.text.Split("\r\n"))
            {
                List<int> rowData = new();
                data.graph.Add(rowData);
                foreach (var c in row.Split("\t"))
                {
                    if (int.TryParse(c, out int v))
                    {
                        rowData.Add(v);
                    }
                }
            }

            data.size = data.graph.Count;
            return;
        }

        var mapObj = Resources.Load<GameObject>($"Maps/map{levelPtr}");
        if (mapObj != null)
        {
            var edit = mapObj.GetComponent<MapEdit>();
            data.name = edit.mapName;

            var agents = mapObj.GetComponentsInChildren<NodeAgent>();
            foreach (var agent in agents)
            {
                data.graph.Add(new());
                data.pos.Add(agent.transform.position);
                data.colors.Add(MyColor.GetColor(agent.color));
            }

            foreach (var row in edit.data.Split("\n"))
            {
                var lst = row.Split("\t");
                if (lst.Length != 2)
                {
                    continue;
                }
                if (int.TryParse(lst[0], out int u) && int.TryParse(lst[1], out int v))
                {
                    data.graph[u].Add(v);
                }
            }

            data.size = data.graph.Count;
            return;
        }

        var num = random.Next(4, 20);
        for (int i = 0; i < num; i++)
        {
            List<int> rowData = new();
            data.graph.Add(rowData);

            Shuffle(num - 1, shuffle);

            var r = Mathf.FloorToInt(-Mathf.Log(1 - (float)random.NextDouble() + 0.0001f));
            var c = Mathf.Clamp(r, 1, num - 2);
            foreach (var v in shuffle)
            {
                if (c > 0)
                {
                    if (v >= i)
                    {
                        rowData.Add(v + 1);
                    }
                    else
                    {
                        rowData.Add(v);
                    }
                    c--;
                }
                else
                {
                    break;
                }
            }
        }

        data.size = data.graph.Count;
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

    public void RemoveNode(Node node, bool split)
    {
        foreach (var e in node.vertex.Neighbors)
        {
            e.Value.value.Hide(node);
        }
        node.Remove();

        node.graphPanel.AddStep(1);

        UpdateStep();

        if (split)
        {
            Split(node.graphPanel);
        }
        else
        {
            UpdateUI();
        }
    }

    private void Split(GraphPanel graphPanel)
    {
        var graph = graphPanel.graph;
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
                /*var counter = step - graphPanel.step;
                graphPanel.SetCounterText(counter.ToString());*/
                graphPanel.SetCounterText(graphPanel.step.ToString());

                if (graphPanel.step + graphPanel.graph.VertexNum <= step)
                {
                    graphPanel.targetColor = MyColor.gray;
                    graphPanel.targetTextColor = MyColor.gray;

                }
                else if (graphPanel.step < step)
                {
                    graphPanel.targetColor = MyColor.green;
                    graphPanel.targetTextColor = MyColor.green;
                }
                else
                {
                    graphPanel.targetColor = MyColor.orange;
                    graphPanel.targetTextColor = MyColor.orange;
                }

                count++;
            }
        }

        stepText.text = step.ToString();
        stepText.color = MyColor.red;
        scoreText.text = score.ToString();
        scoreText.color = MyColor.green;
    }

    public void Create()
    {
        step = 0;
        score = 0;
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

        for (int i = 0; i < data.size; i++)
        {
            var nodeObj = Instantiate(nodePrefab);

            Vector2 relPos;
            if (data.pos.Count > i)
            {
                relPos = data.pos[i];
            }
            else
            {
                var theta = 2 * Mathf.PI / data.size * i;
                relPos = radius * new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));
            }

            var node = nodeObj.GetComponent<Node>();
            node.Init(nodeObj, relPos);

            if (data.colors.Count > i)
            {
                node.targetColor = data.colors[i];
            }
            else
            {
                node.targetColor = MyColor.white;
            }
            graph.AddVertex(node.vertex);
        }


        var ii = 0;
        foreach (var vi in graph.Vertices)
        {
            foreach (var j in data.graph[ii])
            {
                var vj = graph.GetAtIndex(j);

                if (!vi.Value.Adj(vj.Value))
                {
                    var linkObj = Instantiate(linkPrefab);

                    linkObj.transform.rotation = Quaternion.AngleAxis(Vector2.SignedAngle(vj.Value.value.relPos - vi.Value.value.relPos, -Vector2.right), -Vector3.forward);
                    linkObj.transform.position = 0.5f * (vi.Value.value.relPos + vj.Value.value.relPos);

                    var link = linkObj.GetComponent<Link>();
                    link.targetScale = new Vector3((vi.Value.value.relPos - vj.Value.value.relPos).magnitude - 0.35f, 1, 1);
                    link.Init(vi.Value.value, vj.Value.value, linkObj);
                    link.targetColorLeft = vi.Value.value.targetColor;
                    link.targetColorRight = vj.Value.value.targetColor;
                    vi.Value.AddNeighbor(vj.Value, link);
                }
            }
            ii++;
        }

        var graphPanel = CreateGraphPanel(null, graph, 0);
        Split(graphPanel);

        levelText.text = $"Level {levelPtr} - {data.name}";
        levelText.color = MyColor.cyan;

        UpdateUI();
    }

    public void LastLevel()
    {
        levelPtr = Mathf.Clamp(levelPtr - 1, 0, levelNum);
        LoadData();
    }

    public void NextLevel()
    {
        levelPtr = Mathf.Clamp(levelPtr + 1, 0, levelNum);
        LoadData();
    }

    public bool Satisfied
    {
        get
        {
            return graphPanels.Count == 0;
        }
    }

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
        bool flag = false;

        var cur = graphPanels.First;
        while (cur != null)
        {
            var next = cur.Next;

            var graphPanel = cur.Value;
            if (graphPanel.step + graphPanel.graph.VertexNum <= step)
            {
                //score += graphPanel.graph.VertexNum;
                score++;
                graphPanel.Hide();
                graphPool.Return(graphPanel.graph);
                graphPanels.Remove(graphPanel.node);
                flag = true;
            }
            cur = next;
        }

        if (flag)
        {
            UpdateUI();
        }
    }
}
