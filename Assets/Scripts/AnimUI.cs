using TMPro;
using UnityEngine;

public class AnimUI : MonoBehaviour
{
    public ColorEnum color;
    public float speed = 0;
    public float amp = 0; 

    private TMP_Text text;
    private Color targetTextColor;
    private Vector3 targetScale = Vector3.one;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.color = MyColor.zero;
        targetTextColor = MyColor.zero;
    }

    private void Update()
    {
        text.color = Color.Lerp(text.color, targetTextColor, 0.05f);

        targetScale = (1 + amp * Mathf.Cos(Time.time * speed)) * Vector3.one;
        transform.localScale = targetScale;
    }

    public void Show(bool v)
    {
        if (v)
        {
            text.color = MyColor.zero;
            targetTextColor = MyColor.GetColor(color);
        }
        else
        {
            targetTextColor = MyColor.zero;
        }
    }

    public void SetText(object o)
    {
        text.text = o.ToString();
    }

    public void SetColor(Color color)
    {
        targetTextColor = color;
    }
}