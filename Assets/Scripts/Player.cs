using UnityEngine;

public class Player : MonoBehaviour
{
    private Game game;
    private Map map;

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
                        map.RemoveNode(node);
                    }
                }
            }
        }
    }
}
