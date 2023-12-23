using UnityEngine;

public class Game : MonoBehaviour
{
    public Camera _camera;

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
        NewGame();

        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        win = map.Satisfied;

        if (win)
        {
            if (!next)
            {
                _camera.backgroundColor = Color.black;
                next = true;
            }
            else
            {
                timer -= Time.deltaTime;
            }

            if (timer < 0)
            {
                map.NextLevel();
                NewGame();
            }
        }
    }

    public void NewGame()
    {
        started = true;
        win = false;
        next = false;
        timer = 1f;
        map.Create();
        _camera.backgroundColor = Color.black;
    }
}
