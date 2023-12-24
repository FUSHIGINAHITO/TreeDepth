using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public AnimUI title;
    public AnimUI author;
    public AnimUI touch;
    public AnimUI menuTitle;
    public AnimUI level;
    public AnimUI menuButton;
    public AnimUI score;
    public AnimUI scoreValue;
    public AnimUI step;
    public AnimUI stepValue;

    public static UIManager instance => _instance;
    private static UIManager _instance;

    private Dictionary<UIGroup, List<AnimUI>> uiGroups = new();

    private void Awake()
    {
        _instance = this;

        AddGroup(UIGroup.Title, title, author, touch);
        AddGroup(UIGroup.Menu, menuTitle);
        AddGroup(UIGroup.Level, level, menuButton, score, scoreValue, step, stepValue);
    }

    private void AddGroup(UIGroup group, params AnimUI[] uis)
    {
        uiGroups[group] = new(uis);
    }

    public void Show(UIGroup group, bool v)
    {
        foreach (var ui in uiGroups[group])
        {
            ui.Show(v);
        }
    }
}