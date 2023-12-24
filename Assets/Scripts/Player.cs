using UnityEngine;

public class Player : MonoBehaviour
{
    private Game game;
    private Map map;
    float timer = 0;

    private void Awake()
    {
        game = GetComponent<Game>();
        map = GetComponent<Map>();
    }

    private void Update()
    {
        if (game.State == GameState.Title)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                game.EnterState(GameState.Menu);
            }
        }
        else if (game.State == GameState.Menu)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, OurLayer.Mask.GraphPanel);
                if (hit.collider != null)
                {
                    if (hit.collider.transform.parent.TryGetComponent<GraphPanel>(out var graphPanel))
                    {
                        map.ChooseLevel(graphPanel);
                        game.EnterState(GameState.Level);
                    }
                }
            }
            //game.EnterState(GameState.Level);
        }
        else if (game.State == GameState.Level)
        {
            if (!game.Win)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, OurLayer.Mask.Node);
                    if (hit.collider != null)
                    {
                        if (hit.collider.TryGetComponent<Node>(out var node))
                        {
                            map.RemoveNode(node, true);
                            timer = 0.5f;
                        }
                    }
                }

                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                }
                else
                {
                    AutoDelete();
                    timer = 0.5f;
                }
            }
        }
    }

    private void AutoDelete()
    {
        if (!game.Win)
        {
            map.AutoDelete();
        }
    }

    public void BackToMenu()
    {
        if (game.State == GameState.Level && !game.Win)
        {
            game.EnterState(GameState.Menu);
        }
    }
}
