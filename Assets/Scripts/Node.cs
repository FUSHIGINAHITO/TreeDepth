using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Serializable]
    public class Adj
    {
        public Node target;
        public Color linkColor;
    }
    public Color color;
    public List<Adj> adjs;

    public GameObject obj;
    public Vector2 relPos;
    public Graph<Node, Link>.Vertex vertex;
    public GraphPanel graphPanel;
    public Collider2D cld;

    public Vector3 targetScale = Vector3.one;
    private Vector3 scaleVelocity;

    private void Update()
    {
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVelocity, 0.2f, float.MaxValue);
    }

    public void Init(GameObject _obj, Vector2 pos)
    {
        obj = _obj;
        relPos = pos;
        vertex = new(this);
        cld = GetComponent<Collider2D>();
    }

    public void Remove()
    {
        graphPanel.graph.DelVertex(vertex);
        Hide();
    }

    public void Hide()
    {
        cld.enabled = false;
        targetScale = Vector3.zero;
    }
}