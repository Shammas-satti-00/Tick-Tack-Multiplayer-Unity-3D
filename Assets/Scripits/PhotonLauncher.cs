using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    [Tooltip("Name of the game scene to load when room is full")]
    public string gameSceneName = "Game";

    [Header("Testing")]
    public bool skipPhotonAndLoadGame = false;  // Enable this to skip Photon and go straight to game

    private bool isConnecting = false;

    void Start()
    {
        // For testing: Skip Photon entirely and load game directly
        if (skipPhotonAndLoadGame)
        {
            Debug.Log("SKIP PHOTON MODE: Loading game scene directly...");
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        Connect();
    }

    /// <summary>
    /// Connect to Photon Master Server
    /// </summary>
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            isConnecting = true;
            Debug.Log("Connecting to Photon Master Server...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Already connected, attempting to join a room...");
            JoinRandomRoomSafe();
        }
    }

    /// <summary>
    /// Called when connected to Master Server
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");
        PhotonNetwork.AutomaticallySyncScene = true;

        if (isConnecting)
        {
            StartCoroutine(WaitAndJoinRandomRoom());
            isConnecting = false;
        }
    }

    /// <summary>
    /// Coroutine ensures client is fully ready before joining
    /// </summary>
    private IEnumerator WaitAndJoinRandomRoom()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

        Debug.Log("Photon is ready, joining random room...");
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Safe method to join a room externally (e.g., button click)
    /// </summary>
    public void JoinRandomRoomSafe()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Joining random room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log("Photon not ready yet, connecting first...");
            Connect();
        }
    }

    /// <summary>
    /// Called if no random room is available
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random room available, creating a new one.");
        StartCoroutine(WaitAndCreateRoom());
    }

    /// <summary>
    /// Waits until Photon is ready before creating a room
    /// </summary>
    private IEnumerator WaitAndCreateRoom()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
        Debug.Log("Creating new room...");
    }

    /// <summary>
    /// Called when the client joins a room
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room. Players: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Load game scene automatically if room is full
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master loading scene: " + gameSceneName);
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    /// <summary>
    /// Called when another player joins the room
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered: " + newPlayer.NickName + ". Count: " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Room full, Master starting game...");
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    /// <summary>
    /// Called if disconnected from Photon
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Disconnected from Photon: " + cause);
    }

    /// <summary>
    /// Public method to load game scene (can be called by button)
    /// </summary>
    public void LoadGameScene()
    {
        Debug.Log("Loading game scene directly...");
        SceneManager.LoadScene(gameSceneName);
    }

    // Debug display
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;

        string status = $"=== PHOTON LAUNCHER DEBUG ===\n" +
            $"Skip Photon Mode: {skipPhotonAndLoadGame}\n" +
            $"Connected: {PhotonNetwork.IsConnected}\n" +
            $"In Lobby: {PhotonNetwork.InLobby}\n" +
            $"In Room: {PhotonNetwork.InRoom}\n" +
            $"Room Players: {(PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0)}\n" +
            $"Connection State: {PhotonNetwork.NetworkClientState}";

        GUI.Label(new Rect(10, 10, 700, 250), status, style);

        // Add a button to skip to game scene
        if (GUI.Button(new Rect(10, 270, 400, 80), "SKIP TO GAME (TEST)"))
        {
            LoadGameScene();
        }
    }
}