using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject obj;
    public Vector2 relPos;
    public Graph<Node>.Vertex vertex;

    public void Init(GameObject _obj, Vector2 pos)
    {
        obj = _obj;
        relPos = pos;
        vertex = new(this);
    }
}