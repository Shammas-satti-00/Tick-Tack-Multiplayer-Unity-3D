using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public NetworkGridManager gridManager;
    public int x;
    public int y;

    private SpriteRenderer spriteRenderer;
    private bool isOccupied = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetOccupiedAndColor(Color c)
    {
        isOccupied = true;
        spriteRenderer.color = c;
    }

    public void ResetTile()
    {
        isOccupied = false;
        spriteRenderer.color = Color.white;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }
}
