using System;
using UnityEngine;

public class Link : MonoBehaviour
{
    public Node origin;
    public Node target;

    public GameObject obj;
    public Renderer rend;

    private Material mat;

    public void Init(Node _origin, Node _target, GameObject _obj)
    {
        origin = _origin;
        target = _target;
        obj = _obj;
        mat = rend.material;
    }

    private void OnDestroy()
    {
        Destroy(mat);
    }

    public float targetLeftFill = 1;
    public float curLeftFill = 1;
    public float targetRightFill = 1;
    public float curRightFill = 1;
    private float leftVelocity;
    private float rightVelocity; 
    public Vector3 targetScale = Vector3.one;
    private Vector3 scaleVelocity;

    private void Update()
    {
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVelocity, 0.1f, float.MaxValue);

        if (curLeftFill != targetLeftFill)
        {
            curLeftFill = Mathf.SmoothDamp(curLeftFill, targetLeftFill, ref leftVelocity, 0.1f, float.MaxValue);
            mat.SetFloat("_leftFill", curLeftFill);
        }

        {
            if (curRightFill != targetRightFill)
            {
                curRightFill = Mathf.SmoothDamp(curRightFill, targetRightFill, ref rightVelocity, 0.1f, float.MaxValue);
                mat.SetFloat("_rightFill", curRightFill);
            }
        }
    }

    public void Hide(Node node)
    {
        if (node != origin)
        {
            targetLeftFill = 0;
        }
        else
        {
            targetRightFill = 0;
        }
    }

    public void SetColor(Color color)
    {
        mat.SetColor("_color", color);
    }
}