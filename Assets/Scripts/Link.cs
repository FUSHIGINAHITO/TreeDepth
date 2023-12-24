using System;
using UnityEngine;

public class Link : MonoBehaviour
{
    public Node origin;
    public Node target;

    public GameObject obj;
    public Renderer rend;
    private Material mat;
    private Color curColorOrigin = MyColor.zero;
    private Color curColorTarget = MyColor.zero;

    public float targetLeftFill = 1;
    public float curLeftFill = 1;
    public float targetRightFill = 1;
    public float curRightFill = 1;
    private float leftVelocity;
    private float rightVelocity;
    public Vector3 targetScale = Vector3.one;
    private Vector3 scaleVelocity;

    public void Init(Node _origin, Node _target, GameObject _obj)
    {
        origin = _origin;
        target = _target;
        obj = _obj;
        mat = rend.material;
        SetColor(curColorOrigin, curColorTarget);
    }

    private void OnDestroy()
    {
        Destroy(mat);
    }

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



        curColorOrigin = Color.Lerp(curColorOrigin, origin.targetColor, 0.05f);
        curColorTarget = Color.Lerp(curColorTarget, target.targetColor, 0.05f);
        SetColor(curColorOrigin, curColorTarget);
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

    private void SetColor(Color colorOrigin, Color colorTarget)
    {
        mat.SetColor("_color", colorOrigin);
        mat.SetColor("_color2", colorTarget);
    }
}