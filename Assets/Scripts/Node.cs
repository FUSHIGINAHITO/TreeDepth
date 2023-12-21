using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject obj;
    public Vector2 relPos;
    public Graph<Node, Link>.Vertex vertex;
    public GraphPanel graphPanel;

    public void Init(GameObject _obj, Vector2 pos)
    {
        obj = _obj;
        relPos = pos;
        vertex = new(this);
    }

    public void Remove()
    {
        graphPanel.graph.DelVertex(vertex);
        Hide();
    }

    public void Hide()
    {
        obj.SetActive(false);
    }
}