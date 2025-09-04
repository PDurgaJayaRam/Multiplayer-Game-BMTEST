using UnityEngine;
using Unity.Netcode;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Game State")]
    public bool gameStarted = false;
    public bool playersReady = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            // Server initializes the game
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Check if both players are connected
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            playersReady = true;
            StartGameClientRpc();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playersReady = false;
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        gameStarted = true;
        
        // Enable player controls
        EnablePlayerControls();
        
        // Show joystick
        ShowJoystick();
        
        Debug.Log("Game started!");
    }

    private void EnablePlayerControls()
    {
        // Find all player controllers and enable their controls
        CubePlayerController[] players = FindObjectsByType<CubePlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                player.EnableControls(true);
                Debug.Log($"Enabled controls for {player.gameObject.name}");
            }
        }
    }

    private void ShowJoystick()
    {
        // Show the mobile joystick
        if (MobileInputManager.Instance != null && MobileInputManager.Instance.mobileJoystick != null)
        {
            MobileInputManager.Instance.mobileJoystick.gameObject.SetActive(true);
            Debug.Log("Mobile joystick shown");
        }
    }

    public void RestartGame()
    {
        if (IsServer)
        {
            gameStarted = false;
            playersReady = false;
            
            // Reset all players
            CubePlayerController[] players = FindObjectsByType<CubePlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                player.ResetPlayer();
            }
            
            // Restart game on clients
            RestartGameClientRpc();
        }
    }

    [ClientRpc]
    private void RestartGameClientRpc()
    {
        gameStarted = false;
        playersReady = false;
        
        // Hide joystick temporarily
        if (MobileInputManager.Instance != null && MobileInputManager.Instance.mobileJoystick != null)
        {
            MobileInputManager.Instance.mobileJoystick.gameObject.SetActive(false);
        }
        
        // Show restart message
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowGameStatus("Game Restarted!");
        }
    }
}