using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public float fallThreshold = -5f;
    public float respawnHeight = 1f;
    
    [Header("UI Elements")]
    public Text gameStatusText;
    public Button restartButton;
    
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
    
    void Update()
    {
        if (!IsServer) return;
        
        CheckWinCondition();
    }
    
    private void CheckWinCondition()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
            if (player.transform.position.y < fallThreshold)
            {
                // Player fell off the platform
                HandlePlayerFall(player);
            }
        }
    }
    
    private void HandlePlayerFall(GameObject player)
    {
        CubePlayerController controller = player.GetComponent<CubePlayerController>();
        if (controller != null)
        {
            // Determine spawn position
            Vector3 spawnPos = controller.IsHost ? 
                new Vector3(-3f, respawnHeight, 0f) : 
                new Vector3(3f, respawnHeight, 0f);
            
            // Respawn player
            player.transform.position = spawnPos;
            player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            
            // Notify clients
            PlayerFellClientRpc(controller.IsHost);
        }
    }
    
    [ClientRpc]
    private void PlayerFellClientRpc(bool isHostPlayer)
    {
        string playerName = isHostPlayer ? "Red Player" : "Blue Player";
        gameStatusText.text = playerName + " fell off!";
        
        // Clear message after 2 seconds
        Invoke(nameof(ClearStatusMessage), 2f);
    }
    
    private void ClearStatusMessage()
    {
        gameStatusText.text = "";
    }
    
    public void RestartGame()
    {
        if (IsServer)
        {
            // Reset all player positions
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                CubePlayerController controller = player.GetComponent<CubePlayerController>();
                Vector3 spawnPos = controller.IsHost ? 
                    new Vector3(-3f, respawnHeight, 0f) : 
                    new Vector3(3f, respawnHeight, 0f);
                
                player.transform.position = spawnPos;
                player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
            
            RestartGameClientRpc();
        }
    }
    
    [ClientRpc]
    private void RestartGameClientRpc()
    {
        gameStatusText.text = "Game Restarted!";
        Invoke(nameof(ClearStatusMessage), 2f);
    }
}