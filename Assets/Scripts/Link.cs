using System;
using UnityEngine;

public class Link : MonoBehaviour
{
    public GameObject obj;

    public void Init(GameObject _obj)
    {
        obj = _obj;
    }


    public void Hide()
    {
        obj.SetActive(false);
    }
}