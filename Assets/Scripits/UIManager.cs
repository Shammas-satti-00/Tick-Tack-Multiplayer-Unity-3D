using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI turnText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("References")]
    public NetworkGridManager gridManager;

    void Start()
    {
        // Hide game over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Setup restart button
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    /// <summary>
    /// Updates the turn indicator text
    /// </summary>
    public void SetTurnText(int currentPlayer)
    {
        if (turnText != null)
        {
            string playerName = currentPlayer == 1 ? "Player 1 (Red)" : "Player 2 (Blue)";
            turnText.text = $"{playerName}'s Turn";

            // Optional: Color the text based on player
            turnText.color = currentPlayer == 1 ? Color.red : Color.blue;
        }
    }

    /// <summary>
    /// Shows winner message
    /// </summary>
    public void ShowWinner(int winnerPlayer)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
        {
            string playerName = winnerPlayer == 1 ? "Player 1 (Red)" : "Player 2 (Blue)";
            gameOverText.text = $"{playerName} Wins!";
            gameOverText.color = winnerPlayer == 1 ? Color.red : Color.blue;
        }

        // Hide turn text
        if (turnText != null)
            turnText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows draw message
    /// </summary>
    public void ShowDraw()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
        {
            gameOverText.text = "It's a Draw!";
            gameOverText.color = Color.yellow;
        }

        // Hide turn text
        if (turnText != null)
            turnText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when restart button is clicked
    /// </summary>
    void OnRestartClicked()
    {
        if (gridManager != null)
        {
            gridManager.RestartGame();
        }

        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Show turn text again
        if (turnText != null)
            turnText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Optional: Debug display for mobile testing
    /// </summary>
    void OnGUI()
    {
        // Uncomment for debugging on mobile
        /*
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        
        GUI.Label(new Rect(10, 10, 500, 150), 
            $"Connected: {Photon.Pun.PhotonNetwork.IsConnected}\n" +
            $"In Room: {Photon.Pun.PhotonNetwork.InRoom}\n" +
            $"Players: {Photon.Pun.PhotonNetwork.CurrentRoom?.PlayerCount ?? 0}\n" +
            $"Current Turn: {(gridManager != null ? gridManager.currentPlayer : 0)}", 
            style);
        */
    }
}