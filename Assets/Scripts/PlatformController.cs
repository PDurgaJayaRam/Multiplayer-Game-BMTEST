using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [Header("Platform Settings")]
    public float fallThreshold = -5f;
    public float respawnHeight = 1f;
    
    [Header("Visual Settings")]
    public bool showGridLines = true;
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    
    private void Start()
    {
        SetupPlatform();
        
        if (showGridLines)
        {
            CreateGridLines();
        }
    }
    
    private void SetupPlatform()
    {
        // Set platform size
        transform.localScale = new Vector3(20f, 0.5f, 20f);
        transform.position = Vector3.zero;
        
        // Add physics components if missing
        AddPhysicsComponents();
    }
    
    private void AddPhysicsComponents()
    {
        // Add Rigidbody if missing
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure Rigidbody
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.mass = 1f;
        
        // Add Collider if missing
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(20f, 0.5f, 20f);
        }
    }
    
    private void CreateGridLines()
    {
        // Create grid material
        Material gridMaterial = new Material(Shader.Find("Standard"));
        gridMaterial.color = gridColor;
        
        // Create grid lines
        float gridSize = 20f;
        float gridSpacing = 2f;
        
        for (float i = -gridSize/2; i <= gridSize/2; i += gridSpacing)
        {
            // Vertical lines
            CreateLine(new Vector3(i, 0.01f, -gridSize/2), new Vector3(i, 0.01f, gridSize/2), gridMaterial);
            // Horizontal lines
            CreateLine(new Vector3(-gridSize/2, 0.01f, i), new Vector3(gridSize/2, 0.01f, i), gridMaterial);
        }
    }
    
    private void CreateLine(Vector3 start, Vector3 end, Material material)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.parent = transform;
        
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = material;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
    
    private void Update()
    {
        CheckFallenPlayers();
    }
    
    private void CheckFallenPlayers()
    {
        // Find all players in the scene
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        foreach (GameObject player in players)
        {
            if (player.transform.position.y < fallThreshold)
            {
                RespawnPlayer(player);
                ShowFallMessage(player);
            }
        }
    }
    
    private void RespawnPlayer(GameObject player)
    {
        // Get player controller to determine spawn position
        CubePlayerController controller = player.GetComponent<CubePlayerController>();
        if (controller != null)
        {
            // Determine spawn position based on whether player is host or client
            Vector3 spawnPos = controller.IsHost ? 
                new Vector3(-3f, respawnHeight, 0f) : 
                new Vector3(3f, respawnHeight, 0f);
            
            // Reset player position and physics
            player.transform.position = spawnPos;
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            Debug.Log("Respawned player at: " + spawnPos);
        }
    }
    
    private void ShowFallMessage(GameObject player)
    {
        // Show message through MenuManager if available
        if (MenuManager.Instance != null)
        {
            string playerName = player.name;
            MenuManager.Instance.ShowGameStatus(playerName + " fell off!");
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player landed on platform");
        }
    }
    
    // For debugging
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Draw platform boundaries
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(20f, 0.1f, 20f));
            
            // Draw fall threshold
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.down * fallThreshold, 
                               new Vector3(20f, 0.1f, 20f));
        }
    }
}