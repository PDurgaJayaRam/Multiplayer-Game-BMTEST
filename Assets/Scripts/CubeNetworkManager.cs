using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Linq;

public class CubeNetworkManager : MonoBehaviour
{
    public static CubeNetworkManager Instance { get; private set; }
    
    [Header("Network Components")]
    public NetworkManager networkManager;
    public UnityTransport transport;
    
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
    
    void Start()
    {
        // Setup network callbacks
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    
    public void StartHost()
    {
        if (networkManager.StartHost())
        {
            string localIP = GetLocalIPAddress();
            MenuManager.Instance.UpdateStatus("Hosting on: " + localIP);
        }
        else
        {
            MenuManager.Instance.UpdateStatus("Failed to start host");
        }
    }
    
    public void StartClient(string ipAddress)
    {
        // Set connection data
        if (transport != null)
        {
            transport.SetConnectionData(ipAddress, 7777);
        }
        
        if (networkManager.StartClient())
        {
            MenuManager.Instance.UpdateStatus("Connecting to: " + ipAddress);
        }
        else
        {
            MenuManager.Instance.UpdateStatus("Failed to start client");
        }
    }
    
    // Method to start the game
    public void StartGame()
    {
        if (IsServer)
        {
            // Notify all clients that the game is starting
            StartGameClientRpc();
        }
    }
    
    [ClientRpc]
    private void StartGameClientRpc()
    {
        // This will run on all clients
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.StartGame();
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == networkManager.LocalClientId)
        {
            MenuManager.Instance.UpdateStatus("Connected!");
            
            // Show gameplay after a short delay
            Invoke(nameof(ShowGameplay), 1f);
        }
        else
        {
            MenuManager.Instance.UpdateStatus("Player " + clientId + " joined!");
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == networkManager.LocalClientId)
        {
            MenuManager.Instance.UpdateStatus("Disconnected from server");
            Invoke(nameof(ShowMainMenu), 2f);
        }
        else
        {
            MenuManager.Instance.UpdateStatus("Player " + clientId + " disconnected");
        }
    }
    
    private void ShowGameplay()
    {
        MenuManager.Instance.ShowGameplay();
    }
    
    private void ShowMainMenu()
    {
        MenuManager.Instance.ShowMainMenu();
    }
    
    private string GetLocalIPAddress()
    {
        try
        {
            return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                .AddressList.FirstOrDefault(ip => 
                    ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }
    
    // Helper properties
    public bool IsServer => networkManager != null && networkManager.IsServer;
    public bool IsHost => networkManager != null && networkManager.IsHost;
    public bool IsClient => networkManager != null && networkManager.IsClient;
}