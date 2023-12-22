using System;
using UnityEngine;

public class Link : MonoBehaviour
{
    public GameObject obj;

    public void Init(GameObject _obj)
    {
        obj = _obj;
    }

    public Vector3 targetScale = Vector3.one;
    private Vector3 scaleVelocity;

    private void Update()
    {
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVelocity, 0.1f, float.MaxValue);
    }

    public void Hide()
    {
        targetScale = Vector3.zero;
    }
}