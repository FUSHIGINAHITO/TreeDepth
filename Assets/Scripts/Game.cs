using UnityEngine;

public class Game : MonoBehaviour
{
    public Camera _camera;

    public GameState State => state;
    private GameState state = GameState.None;

    public bool Started => started;
    private bool started = false;
    public bool Win => win;
    private bool win = false;
    private Map map;
    private float timer;
    private bool next = false;


    private void Awake()
    {
        map = GetComponent<Map>();
        Application.targetFrameRate = 60;
        _camera.backgroundColor = MyColor.darkGrey;
    }

    private void Start()
    {
        EnterState(GameState.Title);
    }

    private void Update()
    {
        if (state == GameState.Level)
        {
            win = map.Satisfied;

            if (win)
            {
                if (!next)
                {
                    next = true;
                }
                else
                {
                    timer -= Time.deltaTime;
                }

                if (timer < 0)
                {
                    NewGame();

                    if (!map.zen || map.ZenWin || map.ZenLose)
                    {
                        EnterState(GameState.Menu);
                    }
                }
            }
        }
    }

    public void NewGame()
    {
        started = true;
        win = false;
        next = false;
        timer = 1f;
        if (map.zen)
        {
            map.CreateZenLevel();
        }
    }

    public void EnterState(GameState newState)
    {
        switch (state)
        {
            case GameState.None:

                if (newState == GameState.Title)
                {
                    UIManager.instance.Show(UIGroup.Title, true);
                }

                break;
            case GameState.Title:

                if (newState == GameState.Menu)
                {
                    UIManager.instance.Show(UIGroup.Title, false);
                    UIManager.instance.Show(UIGroup.Menu, true);

                    map.CreateMenu();
                }

                break;
            case GameState.Menu:

                if (newState == GameState.Title)
                {
                    UIManager.instance.Show(UIGroup.Title, true);
                    UIManager.instance.Show(UIGroup.Menu, false);
                }
                else if (newState == GameState.Level)
                {
                    UIManager.instance.Show(UIGroup.Menu, false);
                    UIManager.instance.Show(UIGroup.Level, true);

                    NewGame();
                }

                break;
            case GameState.Level:

                if (newState == GameState.Title)
                {
                    UIManager.instance.Show(UIGroup.Title, true);
                    UIManager.instance.Show(UIGroup.Level, false);
                }
                else if (newState == GameState.Menu)
                {
                    UIManager.instance.Show(UIGroup.Menu, true);
                    UIManager.instance.Show(UIGroup.Level, false);

                    map.CreateMenu();
                }

                break;
        }
        state = newState;
    }
}
