using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NodeAgent : MonoBehaviour
{
    public MyColor.ColorEnum color;
    public SpriteRenderer rend;
    public TMP_Text idText;

    public void Init(int _id)
    {
        name = _id.ToString();
        idText.text = _id.ToString();
    }
}