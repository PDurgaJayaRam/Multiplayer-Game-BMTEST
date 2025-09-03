using Unity.Netcode;
using UnityEngine;

public class SimplePlayerSpawner : NetworkBehaviour
{
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    
    // This runs when the network object spawns
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Host spawns Player1
            SpawnPlayer(player1Prefab, new Vector3(-3f, 1f, 0f));
        }
        else
        {
            // Client spawns Player2
            SpawnPlayer(player2Prefab, new Vector3(3f, 1f, 0f));
        }
    }
    
    private void SpawnPlayer(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        
        // Instantiate the player
        GameObject player = Instantiate(prefab, position, Quaternion.identity);
        
        // Get the NetworkObject component
        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        
        if (networkObject != null)
        {
            // Spawn it on the network
            networkObject.Spawn(true);
            Debug.Log("Spawned " + player.name + " at position " + position);
        }
        else
        {
            Debug.LogError("Player prefab missing NetworkObject component!");
            Destroy(player);
        }
    }
}