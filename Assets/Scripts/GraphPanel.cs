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
    public TMP_Text counter2;
    public LinkedListNode<GraphPanel> node;
    public int id;
    public bool isZenEntry;

    public Renderer rend;
    private Material mat;
    public Renderer rend2;
    private Material mat2;

    public Vector3 targetPos;
    private Vector3 followVelocity;
    public Vector3 targetScale;
    private Vector3 scaleVelocity;

    public Color targetColor;
    private Color curColor = MyColor.zero;
    public Color targetTextColor;
    private Color curTextColor = MyColor.zero;
    public Color targetText2Color;
    private Color curText2Color = MyColor.zero;

    private float scaleErr = 0.15f;

    private bool discarded = false;

    private void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, 0.3f, float.MaxValue);
        transform.localScale = Vector3.SmoothDamp(transform.localScale, scaleErr * targetScale, ref scaleVelocity, 0.3f, float.MaxValue);
        curColor = Color.Lerp(curColor, targetColor, 0.05f);
        curTextColor = Color.Lerp(curTextColor, targetTextColor, 0.05f);
        curText2Color = Color.Lerp(curText2Color, targetText2Color, 0.05f);
        SetColor(curColor, curTextColor, curText2Color);

        if (discarded && transform.localScale == Vector3.zero)
        {
            Destroy(gameObject);
        }
    }

    public void Init(Graph<Node, Link> _graph, int _step, bool _isZenEntry)
    {
        SetGraph(_graph);
        step = _step;
        isZenEntry = _isZenEntry;
        mat = rend.material;
        mat2 = rend2.material;

        SetColor(curColor, curTextColor, curText2Color);
        AddStep(0);
    }

    private void OnDestroy()
    {
        Destroy(mat);
        Destroy(mat2);
    }

    public void SetSize(float size)
    {
        ui.localScale = size * Vector3.one;
    }

    public void SetCounterText(object txt)
    {
        counter.text = txt.ToString();
    }

    public void SetCounter2Text(object txt)
    {
        counter2.text = txt.ToString();
    }

    private void SetColor(Color color, Color textColor, Color text2Color)
    {
        foreach (var rend in rends)
        {
            rend.color = color;
        }

        mat.SetColor("_color", color);
        mat2.SetColor("_color", color);

        counter.color = textColor;
        counter2.color = text2Color;
    }

    public void AddStep(int add)
    {
        step += add;
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

    public void AutoSetColor()
    {
        foreach (var v in graph.Vertices)
        {
            v.Value.value.AutoSetColor();
        }
    }

    public void Hide()
    {
        foreach (var rend in rends)
        {
            rend.sortingLayerID = 0;
            rend.sortingOrder = -1;
        }

        targetScale = Vector3.zero;
        targetColor = MyColor.zero;
        targetTextColor = MyColor.zero;
        targetText2Color = MyColor.zero;

        foreach (var v in graph.Vertices)
        {
            v.Value.value.Hide();
        }

        discarded = true;
    }
}