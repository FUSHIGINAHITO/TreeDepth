using UnityEngine;

public class GraphPanel : MonoBehaviour
{
    public Graph<Node> graph;
    public int step;

    public void Init(Graph<Node> _graph, int _step)
    {
        graph = _graph;
        step = _step;
    }
}