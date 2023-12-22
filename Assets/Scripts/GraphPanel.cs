using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphPanel : MonoBehaviour
{
    public Graph<Node, Link> graph;
    public int step = 0;
    public Transform ui;
    public List<SpriteRenderer> rends;
    public TMP_Text counter;
    public LinkedListNode<GraphPanel> node;

    public Vector3 targetPos;
    private Vector3 followVelocity;
    public Vector3 targetScale;
    private Vector3 scaleVelocity;

    private float scaleErr = 0.15f;

    private void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, 0.3f, float.MaxValue);
        transform.localScale = Vector3.SmoothDamp(transform.localScale, scaleErr * targetScale, ref scaleVelocity, 0.3f, float.MaxValue);
    }

    public void Init(Graph<Node, Link> _graph, int _step)
    {
        SetGraph(_graph);
        step = _step;

        AddStep(0);
    }

    public void SetSize(float size)
    {
        ui.localScale = size * Vector3.one;
    }

    private void SetCounterText(string txt)
    {
        counter.text = txt;
    }

    public void SetColor(Color color, Color textColor)
    {
        foreach (var rend in rends)
        {
            rend.color = color;
        }
        counter.color = textColor;
    }

    public void AddStep(int add)
    {
        step += add;
        SetCounterText(step.ToString());
    }

    public void SetGraph(Graph<Node, Link> _graph)
    {
        graph = _graph;

        foreach (var v in graph.Vertices)
        {
            v.Value.value.obj.transform.SetParent(transform, false);
            v.Value.value.graphPanel = this;

            foreach (var e in v.Value.Neighbors)
            {
                e.Value.value.obj.transform.SetParent(transform, false);
            }
        }
    }

    public void Hide()
    {
        targetScale = Vector3.zero;
    }
}