using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject obj;
    public Vector2 relPos;
    public Graph<Node, Link>.Vertex vertex;
    public GraphPanel graphPanel;
    public Collider2D cld;

    public Renderer rend;
    private Material mat;
    public Color targetColor;
    private Color curColor = MyColor.zero;

    public Vector3 targetScale = Vector3.one;
    private Vector3 scaleVelocity;

    private void Update()
    {
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVelocity, 0.2f, float.MaxValue);
        curColor = Color.Lerp(curColor, targetColor, 0.05f);
        SetColor(curColor);
    }

    private void OnDestroy()
    {
        Destroy(mat);
    }

    public void Init(GameObject _obj, Vector2 pos)
    {
        obj = _obj;
        relPos = pos;
        obj.transform.localPosition = relPos;
        vertex = new(this);
        cld = GetComponent<Collider2D>();
        mat = rend.material;
        SetColor(curColor);
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
        targetColor = MyColor.zero;
    }

    private void SetColor(Color color)
    {
        mat.SetColor("_color", color);
    }
}