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
        if (!game.Win)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    var node = hit.collider.GetComponent<Node>();
                    if (node != null)
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

    private void AutoDelete()
    {
        if (!game.Win)
        {
            map.AutoDelete();
        }
    }
}
