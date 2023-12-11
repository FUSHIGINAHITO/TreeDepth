using UnityEngine;

public class Player : MonoBehaviour
{
    private Game game;
    private Map map;
    private Vector2Int pos;

    private void Awake()
    {
        game = GetComponent<Game>();
        map = GetComponent<Map>();
    }

    private void Update()
    {
        
    }
}
