using UnityEngine;

public class MyColor
{
    public static Color blue = new(93 / 255f, 113 / 255f, 170 / 255f);
    public static Color cyan = new(135 / 255f, 197/ 255f, 237/ 255f);
    public static Color red = new(241/ 255f, 182/ 255f, 194/ 255f);
    public static Color green = new(197/ 255f, 224/ 255f, 169/ 255f);
    public static Color magenta = new(222/ 255f, 32/ 255f, 120/ 255f);
    public static Color orange = new(233/ 255f, 203/ 255f, 153/ 255f);
    public static Color yellow = new(244/ 255f, 255/ 255f, 62/ 255f);
    public static Color gray = Color.gray;
    public static Color black = Color.black;
    public static Color white = Color.white;
    public static Color zero = new(0, 0, 0, 0);

    public enum ColorEnum
    {
        white,
        blue,
        cyan,
        red,
        green,
        orange,
        gray,
        black,
        yellow,
    }

    public static Color GetColor(ColorEnum color)
    {
        switch (color)
        {
            case ColorEnum.blue:
                return blue;
            case ColorEnum.cyan:
                return cyan;
            case ColorEnum.red:
                return red;
            case ColorEnum.green:
                return green;
            case ColorEnum.orange:
                return orange;
            case ColorEnum.gray:
                return gray;
            case ColorEnum.black:
                return black;
            case ColorEnum.white:
                return white;
            case ColorEnum.yellow:
                return yellow;
        }
        return zero;
    }
}