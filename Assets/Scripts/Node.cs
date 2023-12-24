using UnityEngine;

public class Node : MonoBehaviour
{
    public GameObject obj;
    public Vector2 relPos;
    public Graph<Node, Link>.Vertex vertex;
    public GraphPanel graphPanel;
    public Collider2D cld;
    public int id;

    public Renderer rend;
    private Material mat;
    public bool autoColor = true;
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

    public void Init(GameObject _obj, Vector2 pos, int _id)
    {
        obj = _obj;
        relPos = pos;
        obj.transform.localPosition = relPos;
        vertex = new(this);
        cld = GetComponent<Collider2D>();
        mat = rend.material;
        SetColor(curColor);
        targetColor = MyColor.white;
        id = _id;
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

    public void AutoSetColor()
    {
        if (autoColor)
        {
            float t = 1;
            float maxDeg = graphPanel.graph.MaxDegree;
            float minDeg = graphPanel.graph.MinDegree;
            if (maxDeg - minDeg > 0)
            {
                t = (vertex.Degree - minDeg) / (maxDeg - minDeg);
            }

            targetColor = Color.Lerp(MyColor.gray, MyColor.white, t);
        }
    }
}