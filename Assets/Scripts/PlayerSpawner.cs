using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject redCubePrefab; // Host prefab
    public GameObject blueCubePrefab; // Client prefab

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log("PlayerSpawner: OnNetworkSpawn called");

        if (IsServer)
        {
            Debug.Log("PlayerSpawner: IsServer is true, spawning host player");
            // Server spawns both players
            SpawnPlayerForHost();
        }
        else
        {
            Debug.Log("PlayerSpawner: IsServer is false, not spawning");
        }
    }

    private void SpawnPlayerForHost()
    {
        Debug.Log("PlayerSpawner: SpawnPlayerForHost called");
        // Spawn red cube for host
        SpawnPlayer(redCubePrefab, new Vector3(-3f, 1f, 0f), true);
    }

    public void SpawnPlayerForClient(ulong clientId)
    {
        Debug.Log($"PlayerSpawner: SpawnPlayerForClient called for client {clientId}");
        // Spawn blue cube for client
        SpawnPlayer(blueCubePrefab, new Vector3(3f, 1f, 0f), false);
    }

    private void SpawnPlayer(GameObject prefab, Vector3 position, bool isHost)
    {
        Debug.Log($"PlayerSpawner: SpawnPlayer called with prefab: {prefab?.name}, position: {position}, isHost: {isHost}");

        if (prefab == null)
        {
            Debug.LogError("PlayerSpawner: Prefab is null!");
            return;
        }

        GameObject player = Instantiate(prefab, position, Quaternion.identity);
        Debug.Log($"PlayerSpawner: Player instantiated: {player.name}");

        NetworkObject networkObject = player.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            Debug.Log("PlayerSpawner: NetworkObject found");
            // Set player as host or client
            CubePlayerController controller = player.GetComponent<CubePlayerController>();
            if (controller != null)
            {
                Debug.Log($"PlayerSpawner: Setting IsHostPlayer to {isHost}");
                controller.IsHostPlayer = isHost;
            }
            else
            {
                Debug.LogError("PlayerSpawner: CubePlayerController not found!");
            }

            // Spawn the player
            Debug.Log("PlayerSpawner: Calling networkObject.Spawn");
            networkObject.Spawn(true);
            
            // FIXED: Give ownership to the appropriate client
            if (isHost)
            {
                // Host player should be owned by the server/host
                networkObject.ChangeOwnership(NetworkManager.ServerClientId);
                Debug.Log($"PlayerSpawner: Host player ownership given to server (ID: {NetworkManager.ServerClientId})");
            }
            else
            {
                // Client player should be owned by the connecting client
                // This will be handled when the client connects
                Debug.Log("PlayerSpawner: Client player spawned, waiting for client connection");
            }
            
            Debug.Log($"PlayerSpawner: Spawned {prefab.name} at position {position}");
        }
        else
        {
            Debug.LogError("PlayerSpawner: NetworkObject component not found!");
            Destroy(player);
        }
    }
}