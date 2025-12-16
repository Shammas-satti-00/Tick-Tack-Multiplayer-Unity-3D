// NetworkGridManager.cs
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkGridManager : MonoBehaviourPunCallbacks
{
    [Header("Grid Size (set to 3x3 for TicTacToe)")]
    public int width = 3;
    public int height = 3;

    [Header("Tile Settings")]
    public GameObject tilePrefab;
    [Range(0.5f, 1f)]
    public float tileScale = 0.92f;

    [Header("Players")]
    public Color player1Color = Color.red;   // MasterClient (Player 1 / X)
    public Color player2Color = Color.blue;  // Other (Player 2 / O)

    [Header("References")]
    public UIManager uiManager;  // assign in inspector

    [Header("Testing")]
    public bool offlineTestMode = true;  // Set to false when testing multiplayer

    private int[,] board;   // 0 = empty, 1 = player1, 2 = player2
    private bool gameOver = false;

    // currentPlayer is 1 or 2 indicating whose turn it is
    // Start with player 1 (MasterClient)
    public int currentPlayer = 1;

    // local player's id (1 or 2)
    private int localPlayerId;

    void Start()
    {
        board = new int[width, height];

        Debug.Log("=== STARTING GRID GENERATION ===");
        Debug.Log($"Tile Prefab assigned? {tilePrefab != null}");
        Debug.Log($"UIManager assigned? {uiManager != null}");
        Debug.Log($"Offline Test Mode: {offlineTestMode}");

        GenerateGrid();

        Debug.Log($"Grid generated! Children count: {transform.childCount}");

        // Determine local player id
        if (offlineTestMode || !PhotonNetwork.IsConnected)
        {
            // Offline testing - allow both players to be controlled locally
            localPlayerId = 1;
            Debug.Log("[GridManager] OFFLINE TEST MODE: You can play both sides");
        }
        else
        {
            localPlayerId = PhotonNetwork.IsMasterClient ? 1 : 2;
            Debug.Log($"[GridManager] ONLINE MODE: You are Player {localPlayerId}");
        }

        Debug.Log($"[GridManager] Start: localPlayerId={localPlayerId}, currentPlayer={currentPlayer}");

        if (uiManager != null)
            uiManager.SetTurnText(currentPlayer);
    }

    void Update()
    {
        // Detect mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPos.z = 0; // Ensure Z matches your tiles

            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null)
            {
                Tile tile = hit.GetComponent<Tile>();
                if (tile != null)
                {
                    NetworkTileClicked(tile.x, tile.y);
                }
            }
        }

        // Touch input for mobile
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            touchWorldPos.z = 0;

            Collider2D hit = Physics2D.OverlapPoint(touchWorldPos);
            if (hit != null)
            {
                Tile tile = hit.GetComponent<Tile>();
                if (tile != null)
                {
                    NetworkTileClicked(tile.x, tile.y);
                }
            }
        }
    }

    void GenerateGrid()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned!");
            return;
        }

        float xOffset = (width - 1) / 2f;
        float yOffset = (height - 1) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x - xOffset, y - yOffset);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile {x} {y}";
                tile.transform.localScale = new Vector3(tileScale, tileScale, 1f);

                Tile tileScript = tile.GetComponent<Tile>();
                if (tileScript != null)
                {
                    tileScript.gridManager = this;
                    tileScript.x = x;
                    tileScript.y = y;
                }

                Debug.Log($"Created tile at ({x}, {y}) - Position: {position}");
            }
        }
    }

    // Called by Tile when clicked — local attempt to make a move.
    public void NetworkTileClicked(int x, int y)
    {
        Debug.Log($"[GridManager] NetworkTileClicked({x}, {y}) called | localPlayer={localPlayerId}, currentPlayer={currentPlayer}");

        if (gameOver)
        {
            Debug.Log("[GridManager] Game is over → click ignored");
            return;
        }

        // In offline test mode, allow any move
        if (!offlineTestMode && localPlayerId != currentPlayer)
        {
            Debug.Log("[GridManager] Not your turn → click ignored");
            return;
        }

        if (board[x, y] != 0)
        {
            Debug.Log("[GridManager] Tile already occupied");
            return;
        }

        Debug.Log("[GridManager] Processing move...");

        // If offline or not connected, call RPC directly
        if (offlineTestMode || !PhotonNetwork.IsConnected || photonView == null)
        {
            RPC_MarkCell(x, y, currentPlayer);
        }
        else
        {
            photonView.RPC("RPC_MarkCell", RpcTarget.AllBuffered, x, y, currentPlayer);
        }
    }

    [PunRPC]
    void RPC_MarkCell(int x, int y, int player)
    {
        Debug.Log($"[GridManager] RPC_MarkCell({x}, {y}, {player})");

        if (board[x, y] != 0)
        {
            Debug.Log("[GridManager] RPC ignored → tile already set");
            return;
        }

        // Mark the board
        board[x, y] = player;

        // Update visual
        Transform child = transform.Find($"Tile {x} {y}");
        Debug.Log($"[GridManager] Find Tile: {(child != null ? "SUCCESS" : "FAILED")}");

        if (child != null)
        {
            Tile tile = child.GetComponent<Tile>();
            Debug.Log($"[GridManager] Tile component found? {tile != null}");

            if (tile != null)
            {
                Color targetColor = player == 1 ? player1Color : player2Color;
                tile.SetOccupiedAndColor(targetColor);
                Debug.Log($"[GridManager] Set tile color to {targetColor}");
            }
        }

        // Just switch turns (win/draw logic disabled for testing)
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        Debug.Log($"[GridManager] Turn switched to player {currentPlayer}");

        if (uiManager != null)
            uiManager.SetTurnText(currentPlayer);
    }

    // Win-check for 3-in-a-row (TicTacToe)
    bool CheckWin(int x, int y, int player)
    {
        // Horizontal, vertical, diagonal
        return (CountInDirection(x, y, 1, 0, player) + CountInDirection(x, y, -1, 0, player) - 1 >= 3) ||
               (CountInDirection(x, y, 0, 1, player) + CountInDirection(x, y, 0, -1, player) - 1 >= 3) ||
               (CountInDirection(x, y, 1, 1, player) + CountInDirection(x, y, -1, -1, player) - 1 >= 3) ||
               (CountInDirection(x, y, 1, -1, player) + CountInDirection(x, y, -1, 1, player) - 1 >= 3);
    }

    int CountInDirection(int x, int y, int dx, int dy, int player)
    {
        int count = 0;
        int cx = x;
        int cy = y;

        while (cx >= 0 && cx < width && cy >= 0 && cy < height && board[cx, cy] == player)
        {
            count++;
            cx += dx;
            cy += dy;
        }

        return count;
    }

    bool IsBoardFull()
    {
        for (int xx = 0; xx < width; xx++)
            for (int yy = 0; yy < height; yy++)
                if (board[xx, yy] == 0)
                    return false;
        return true;
    }

    public void RestartGame()
    {
        // In offline mode or if we're master client
        if (offlineTestMode || !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.IsConnected && photonView != null)
                photonView.RPC("RPC_Restart", RpcTarget.AllBuffered);
            else
                RPC_Restart();
        }
    }

    [PunRPC]
    void RPC_Restart()
    {
        gameOver = false;
        currentPlayer = 1;
        board = new int[width, height];

        // Reset visuals
        foreach (Transform child in transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null)
                tile.ResetTile();
        }

        if (uiManager != null)
            uiManager.SetTurnText(currentPlayer);
    }

    // Debug display - shows connection and game state on screen
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;

        string status = $"=== TIC TAC TOE DEBUG ===\n" +
            $"Offline Test Mode: {offlineTestMode}\n" +
            $"Connected: {PhotonNetwork.IsConnected}\n" +
            $"In Room: {PhotonNetwork.InRoom}\n" +
            $"Room Players: {PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}\n" +
            $"My Player ID: {localPlayerId}\n" +
            $"Current Turn: {currentPlayer}\n" +
            $"Game Over: {gameOver}\n" +
            $"Tiles Generated: {transform.childCount}";

        GUI.Label(new Rect(10, 10, 700, 300), status, style);
    }
}