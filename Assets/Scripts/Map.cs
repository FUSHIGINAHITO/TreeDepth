using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject squarePrefab;
    public GameObject nodePrefab;
    public GameObject linkPrefab;
    public GameObject graphPanelPrefab;
    public bool debug = false;

    public float H = 50;
    public float W = 70;
    public List<Vector2> res = new();

    private int curLevel = 0;
    private int levelNum;

    public bool zen = false;
    private int curZenLevel = 0;
    public int maxZenLevel = 99;

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

    private List<LevelData> allData = new();
    private LevelData curData;
    private LevelData curZenData = new();

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
        LoadAllLevelData();

#if !UNITY_EDITOR
        debug = false;
#endif
    }

    private void LoadAllLevelData()
    {
        int id = 1;
        while (true)
        {
            var txt = Resources.Load<TextAsset>($"Maps/map{id}");
            if (txt != null)
            {
                LevelData data = new();
                allData.Add(data);

                int count = 0;
                bool tikz = false;
                foreach (var row in txt.text.Split("\r\n"))
                {
                    if (count > 0)
                    {
                        if (count == 1)
                        {
                            tikz = row.Length > 0 && row[0] == '\\';
                        }

                        if (!tikz)
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
                        else
                        {
                            var col = row.Split(" ");
                            if (col.Length > 0 && col[0] == "\\node[main_node]")
                            {
                                data.graph.Add(new());
                                if (float.TryParse(col[3].Trim('(', ','), out var x) && float.TryParse(col[4].Trim(')'), out var y))
                                {
                                    data.pos.Add(0.4f * new Vector2(x, y));
                                }
                            }
                            else if (col.Length > 2 && col[1] == "edge" && col[2] == "node")
                            {
                                if (int.TryParse(col[0].Trim('(', ')'), out var i) && int.TryParse(col[4].Trim('(', ')'), out var j))
                                {
                                    data.graph[i].Add(j);
                                }
                            }
                        }
                    }
                    else
                    {
                        data.name = row;
                    }
                    count++;
                }

                data.size = data.graph.Count;
                id++;
                continue;
            }

            var mapObj = Resources.Load<GameObject>($"Maps/map{id}");
            if (mapObj != null)
            {
                LevelData data = new();
                allData.Add(data);

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
                id++;
                continue;
            }

            break;
        }

        levelNum = allData.Count;
    }

    private void CreateZenData()
    {
        curZenData.Clear();

        var penalty = 2 * ((score - 10) / 10) + 1;
        var num = Mathf.Clamp(random.Next(7, 15) + penalty, 0, 20);
        for (int i = 0; i < num; i++)
        {

            List<int> rowData = new();
            curZenData.graph.Add(rowData);

            Shuffle(num - 1, shuffle);

            var r = Mathf.FloorToInt(-Mathf.Log(1 - (float)random.NextDouble() + 0.0001f));
            var c = Mathf.Clamp(r + penalty, 1, num - 2);
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

        curZenData.size = curZenData.graph.Count;
    }

    private void Shuffle(int k, List<int> res)
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

        score--;

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

    private void UpdateUI(bool menu = false)
    {
        if (graphPanels.Count > 0)
        {
            GetLayout(graphPanels.Count, W, H, res, out float a);

            int count = 0;
            foreach (var graphPanel in graphPanels)
            {
                graphPanel.targetPos = res[count];
                graphPanel.targetScale = a * Vector3.one;
                if (!menu)
                {
                    graphPanel.AutoSetColor();
                }

                if (menu)
                {
                    if (!graphPanel.isZenEntry)
                    {
                        var minStep = ArchiveManager.instance.archive.GetStep(graphPanel.id);
                        if (minStep != null)
                        {
                            graphPanel.SetCounterText(minStep.Value);
                            graphPanel.targetTextColor = MyColor.red;
                            graphPanel.targetColor = MyColor.orange;
                        }
                        else
                        {
                            graphPanel.SetCounterText(graphPanel.graph.VertexNum);
                            graphPanel.targetTextColor = MyColor.gray;
                            graphPanel.targetColor = MyColor.gray;
                        }
                        graphPanel.SetCounter2Text(graphPanel.id);
                    }
                    else
                    {
                        var maxZen = ArchiveManager.instance.archive.ZenMaxLevel;
                        if (maxZen >= 0)
                        {
                            graphPanel.SetCounterText(maxZen);
                            graphPanel.targetTextColor = MyColor.red;
                            graphPanel.targetColor = MyColor.cyan;
                        }
                        else
                        {
                            graphPanel.SetCounterText("?");
                            graphPanel.targetTextColor = MyColor.gray;
                            graphPanel.targetColor = MyColor.gray;
                        }
                        graphPanel.SetCounter2Text("¿");
                    }
                }
                else
                {
                    graphPanel.SetCounterText(graphPanel.step);
                    graphPanel.SetCounter2Text(step - graphPanel.step);
                }

                if (menu)
                {
                    graphPanel.targetText2Color = MyColor.green;
                }
                else
                {
                    if (graphPanel.step + graphPanel.graph.VertexNum <= step)
                    {
                        graphPanel.targetColor = MyColor.gray;
                        graphPanel.targetTextColor = MyColor.gray;
                        graphPanel.targetText2Color = MyColor.gray;

                        foreach (var v in graphPanel.graph.Vertices)
                        {
                            v.Value.value.targetColor = MyColor.gray;
                        }
                    }
                    else if (graphPanel.step < step)
                    {
                        graphPanel.targetColor = MyColor.green;
                        graphPanel.targetTextColor = MyColor.red;
                        graphPanel.targetText2Color = MyColor.green;
                    }
                    else
                    {
                        graphPanel.targetColor = MyColor.orange;
                        graphPanel.targetTextColor = MyColor.red;
                        graphPanel.targetText2Color = MyColor.green;
                    }
                }

                count++;
            }
        }

        if (!menu)
        {
            if (!zen)
            {
                UIManager.instance.stepValue.SetText(step);
                UIManager.instance.stepValue.SetColor(MyColor.red);
            }
            else
            {
                UIManager.instance.stepValue.SetText(score);
                UIManager.instance.stepValue.SetColor(MyColor.green);
            }
        }
    }


    private void ClearPanels()
    {
        foreach (var graphPanel in graphPanels)
        {
            graphPanel.Hide();
            graphPool.Return(graphPanel.graph);
        }
        graphPanels.Clear();
    }

    public void CreateZenLevel()
    {
        curZenLevel++;
        if (curZenLevel <= maxZenLevel)
        {
            GraphPanel zenPanel = null;
            if (graphPanels.Count > 0)
            {
                zenPanel = graphPanels.Last.Value;
                ClearPanels();
                if (!zenPanel.isZenEntry)
                {
                    zenPanel = null;
                }
            }

            CreateZenData();
            step = 0;

            var graph = CreateGraph(curZenData);
            var graphPanel = CreateGraphPanel(zenPanel, graph, 0);
            Split(graphPanel);

            UIManager.instance.level.SetText($"Zen {curZenLevel}");

            UpdateUI();
        }
    }

    public void Restore()
    {
        step = 0;

        if (!zen)
        {
            score = 0;
            UIManager.instance.level.SetText($"#{curLevel} - {curData.name}");
            UIManager.instance.step.SetText("Depth");
        }
        else
        {
            score = 10;
            curZenLevel = 0;
            UIManager.instance.step.SetText("Soul");
        }
    }

    public void ChooseLevel(GraphPanel graphPanel)
    {
        curLevel = graphPanel.id;

        if (curLevel <= levelNum)
        {
            curData = allData[curLevel - 1];
            zen = false;
        }
        else
        {
            CreateZenData();
            zen = true;
        }

        var cur = graphPanels.First;
        while (cur != null)
        {
            var next = cur.Next;

            if (cur.Value != graphPanel)
            {
                cur.Value.Hide();
                graphPool.Return(cur.Value.graph);
                graphPanels.Remove(cur);
            }
            cur = next;
        }

        Restore();

        UpdateUI();
    }

    /*public void ChooseLevel(int id)
    {
        levelPtr = id;
        curData = allData[levelPtr - 1];
    }

    public void LastLevel()
    {
        levelPtr = Mathf.Clamp(levelPtr - 1, 1, levelNum);
        curData = allData[levelPtr - 1];
    }

    public void NextLevel()
    {
        levelPtr = Mathf.Clamp(levelPtr + 1, 1, levelNum);
        curData = allData[levelPtr - 1];
    }*/

    public bool Satisfied
    {
        get
        {
            return graphPanels.Count == 0;
        }
    }

    public bool ZenWin
    {
        get
        {
            return zen && curZenLevel > maxZenLevel;
        }
    }

    public bool ZenLose
    {
        get
        {
            return zen && score < 0;
        }
    }

    private void UpdateStep()
    {
        int old = step;
        foreach (var graphPanel in graphPanels)
        {
            if (graphPanel.step > step)
            {
                step = graphPanel.step;
            }
        }
        if (step > old)
        {
            //score -= step - old;
        }
    }

    private GraphPanel CreateGraphPanel(GraphPanel oldPanel, Graph<Node, Link> graph, int step, bool isZenEntry = false)
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

        graphPanel.Init(graph, step, isZenEntry);

        if (oldPanel == null || !graphPanels.Contains(oldPanel))
        {
            graphPanel.node = graphPanels.AddLast(graphPanel);
            graphPanel.id = graphPanels.Count;
        }
        else
        {
            graphPanel.node = graphPanels.AddAfter(oldPanel.node, graphPanel);
        }

        return graphPanel;
    }

    private Graph<Node, Link> CreateGraph(LevelData data)
    {
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
            node.Init(nodeObj, relPos, i);
            if (data.colors.Count > node.id)
            {
                node.targetColor = data.colors[node.id];
                node.autoColor = false;
            }
            else
            {
                node.autoColor = true;
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
                    link.targetScale = new Vector3((vi.Value.value.relPos - vj.Value.value.relPos).magnitude - 0.2f, 1, 1);
                    link.Init(vi.Value.value, vj.Value.value, linkObj);
                    vi.Value.AddNeighbor(vj.Value, link);
                }
            }
            ii++;
        }

        return graph;
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

    public void CreateMenu()
    {
        ClearPanels();

        var maxLevel = ArchiveManager.instance.archive.MaxLevel;
        var zenUnlock = false;
        if (maxLevel < 0)
        {
            maxLevel = 1;
        }
        else
        {
            if (maxLevel == levelNum)
            {
                zenUnlock = true;
            }
            maxLevel++;
        }
        maxLevel = Mathf.Max(6, maxLevel);

        if (debug)
        {
            maxLevel = int.MaxValue;
            zenUnlock = true;
        }

        foreach (var data in allData)
        {
            if (maxLevel > 0)
            {
                var graph = CreateGraph(data);
                CreateGraphPanel(null, graph, 0);
                maxLevel--;
            }
        }

        if (zenUnlock)
        {
            CreateGraphPanel(null, graphPool.Require(), 0, true);
        }

        UpdateUI(true);
    }

    public void UpdateArchive(bool forceOver)
    {
        if (!forceOver)
        {
            if (zen)
            {
                ArchiveManager.instance.archive.ZenMaxLevel = curZenLevel;
                ArchiveManager.instance.archive.ZenMaxScore = score;
            }
            else
            {
                ArchiveManager.instance.archive.MaxLevel = curLevel;
                ArchiveManager.instance.archive.SetStep(curLevel, step);
            }
        }
        else
        {
            if (zen)
            {
                ArchiveManager.instance.archive.ZenMaxLevel = curZenLevel - 1;
            }
        }

        ArchiveManager.instance.SaveArchive();
    }
}
