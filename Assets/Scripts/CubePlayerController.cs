using Unity.Netcode;
using UnityEngine;

public class CubePlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 5f;
    public float maxSpeed = 8f;
    
    [Header("Physics Settings")]
    public float pushForce = 10f;
    public float mass = 1f;
    
    [Header("Network Sync")]
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
    private NetworkVariable<Vector3> networkVelocity = new NetworkVariable<Vector3>();
    
    [Header("Visual Settings")]
    public Color hostColor = Color.red;
    public Color clientColor = Color.blue;
    public ParticleSystem collisionParticles;
    
    // Components
    private Rigidbody rb;
    private Renderer playerRenderer;
    private TrailRenderer trailRenderer;
    
    // Input state
    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private bool isLocalPlayer;
    
    // Reference to MenuManager
    private MenuManager menuManager;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Initialize components
        InitializeComponents();
        
        // Set player properties
        SetPlayerProperties();
        
        // Subscribe to network variable changes
        SubscribeToNetworkVariables();
        
        // Get MenuManager reference
        menuManager = MenuManager.Instance;
        
        // Enable/disable controls based on ownership and connection state
        UpdateControlState();
    }
    
    private void InitializeComponents()
    {
        // Get required components
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        
        // Configure rigidbody
        if (rb != null)
        {
            rb.mass = mass;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = !IsOwner;
        }
        
        // Configure trail renderer
        if (trailRenderer != null)
        {
            trailRenderer.emitting = IsOwner;
        }
    }
    
    private void SetPlayerProperties()
    {
        isLocalPlayer = IsOwner;
        
        // Set player color based on host/client status
        if (playerRenderer != null)
        {
            playerRenderer.material.color = IsHost ? hostColor : clientColor;
        }
        
        // Set player name for debugging
        gameObject.name = IsHost ? "Player_Host" : "Player_Client";
        
        Debug.Log($"{gameObject.name} spawned. IsOwner: {IsOwner}, IsHost: {IsHost}");
    }
    
    private void SubscribeToNetworkVariables()
    {
        networkPosition.OnValueChanged += OnPositionChanged;
        networkRotation.OnValueChanged += OnRotationChanged;
        networkVelocity.OnValueChanged += OnVelocityChanged;
    }
    
    private void UpdateControlState()
    {
        // Enable controls for local player when connected
        bool controlsEnabled = IsOwner && menuManager != null && menuManager.isConnected;
        
        if (rb != null)
        {
            rb.isKinematic = !controlsEnabled;
        }
        
        if (trailRenderer != null)
        {
            trailRenderer.emitting = controlsEnabled;
        }
    }
    
    void Update()
    {
        // Only handle input for local player when connected
        if (!IsOwner || menuManager == null || !menuManager.isConnected) return;
        
        HandleInput();
        UpdateVisualEffects();
    }
    
    void FixedUpdate()
    {
        // Only apply movement for local player when connected
        if (!IsOwner || menuManager == null || !menuManager.isConnected) return;
        
        ApplyMovement();
        SyncNetworkState();
    }
    
    private void HandleInput()
    {
        // Reset input
        moveInput = Vector3.zero;
        
        // Get input from MobileInputManager
        if (MobileInputManager.Instance != null)
        {
            Vector2 movementInput = MobileInputManager.Instance.GetMovementInput();
            moveInput = new Vector3(movementInput.x, 0, movementInput.y);
        }
        
        // Fallback to keyboard input for editor testing
        if (Application.isEditor)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 keyboardInput = new Vector3(horizontal, 0, vertical);
            
            // Use keyboard input if it's stronger than touch input
            if (keyboardInput.magnitude > moveInput.magnitude)
            {
                moveInput = keyboardInput;
            }
        }
    }
    
    private void ApplyMovement()
    {
        if (rb == null) return;
        
        // Calculate target velocity
        Vector3 targetVelocity = moveInput * moveSpeed;
        
        // Smooth acceleration
        if (moveInput.magnitude > 0)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Apply deceleration when no input
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }
        
        // Clamp to max speed
        if (currentVelocity.magnitude > maxSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxSpeed;
        }
        
        // Apply velocity to rigidbody
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }
    
    private void SyncNetworkState()
    {
        if (IsServer)
        {
            // Server updates network variables
            networkPosition.Value = transform.position;
            networkRotation.Value = transform.rotation;
            networkVelocity.Value = rb.linearVelocity;
        }
        else
        {
            // Client sends position to server
            if (currentVelocity.magnitude > 0.1f)
            {
                UpdatePositionServerRpc(transform.position, rb.linearVelocity);
            }
        }
    }
    
    private void UpdateVisualEffects()
    {
        // Update trail renderer based on movement
        if (trailRenderer != null)
        {
            trailRenderer.emitting = currentVelocity.magnitude > 0.5f;
        }
    }
    
    // Network variable callbacks
    private void OnPositionChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (!IsOwner)
        {
            transform.position = newValue;
        }
    }
    
    private void OnRotationChanged(Quaternion previousValue, Quaternion newValue)
    {
        if (!IsOwner)
        {
            transform.rotation = newValue;
        }
    }
    
    private void OnVelocityChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (!IsOwner && rb != null)
        {
            rb.linearVelocity = newValue;
        }
    }
    
    // Network RPCs
    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 position, Vector3 velocity)
    {
        networkPosition.Value = position;
        networkVelocity.Value = velocity;
    }
    
    [ServerRpc]
    private void PushPlayerServerRpc(ulong targetPlayerId, Vector3 pushForce)
    {
        // Find the target player and apply force
        foreach (var player in FindObjectsOfType<CubePlayerController>())
        {
            if (player.GetComponent<NetworkObject>().NetworkObjectId == targetPlayerId)
            {
                Rigidbody targetRb = player.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    targetRb.AddForce(pushForce, ForceMode.Impulse);
                    Debug.Log($"Pushed player {targetPlayerId} with force {pushForce}");
                }
                break;
            }
        }
    }
    
    // Collision handling
    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner || menuManager == null || !menuManager.isConnected) return;
        
        // Handle player-to-player collision
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
        
        // Handle platform collision
        if (collision.gameObject.CompareTag("Platform"))
        {
            HandlePlatformCollision(collision);
        }
    }
    
    private void HandlePlayerCollision(Collision collision)
    {
        // Only handle collisions if game has started
        if (menuManager != null && menuManager.gameStarted)
        {
            CubePlayerController otherPlayer = collision.gameObject.GetComponent<CubePlayerController>();
            if (otherPlayer != null)
            {
                // Calculate push direction and force
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                Vector3 pushForceVector = pushDirection * pushForce;
                
                // Apply push locally
                Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
                if (otherRb != null)
                {
                    otherRb.AddForce(pushForceVector, ForceMode.Impulse);
                }
                
                // Sync push over network
                PushPlayerServerRpc(
                    otherPlayer.GetComponent<NetworkObject>().NetworkObjectId, 
                    pushForceVector
                );
                
                // Play collision effects
                PlayCollisionEffects(collision.contacts[0].point);
                
                Debug.Log($"{gameObject.name} collided with {collision.gameObject.name}");
            }
        }
    }
    
    private void HandlePlatformCollision(Collision collision)
    {
        // Only handle platform effects if game has started
        if (menuManager != null && menuManager.gameStarted)
        {
            // Play landing effects if falling
            if (rb.linearVelocity.y < -2f)
            {
                PlayLandingEffects(collision.contacts[0].point);
            }
        }
    }
    
    private void PlayCollisionEffects(Vector3 position)
    {
        // Only play effects if game has started
        if (menuManager != null && menuManager.gameStarted)
        {
            // Play particle effects
            if (collisionParticles != null)
            {
                Instantiate(collisionParticles, position, Quaternion.identity);
            }
            
            // Add screen shake (if you have a camera controller)
            // CameraShake.Instance.Shake(0.1f, 0.1f);
        }
    }
    
    private void PlayLandingEffects(Vector3 position)
    {
        // Only play effects if game has started
        if (menuManager != null && menuManager.gameStarted)
        {
            // Play landing effects
            Debug.Log($"{gameObject.name} landed at {position}");
        }
    }
    
    // Public methods for external control
    public void SetMovementSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    public void SetPushForce(float newForce)
    {
        pushForce = newForce;
    }
    
    public void ResetPlayer()
    {
        // Reset position and velocity
        Vector3 spawnPos = IsHost ? new Vector3(-3f, 1f, 0f) : new Vector3(3f, 1f, 0f);
        transform.position = spawnPos;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        currentVelocity = Vector3.zero;
        moveInput = Vector3.zero;
        
        Debug.Log($"{gameObject.name} reset to spawn position");
    }
    
    // Cleanup
    public override void OnNetworkDespawn()
    {
        // Unsubscribe from network variable changes
        networkPosition.OnValueChanged -= OnPositionChanged;
        networkRotation.OnValueChanged -= OnRotationChanged;
        networkVelocity.OnValueChanged -= OnVelocityChanged;
        
        base.OnNetworkDespawn();
    }
    
    // Gizmos for debugging
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && IsOwner)
        {
            // Draw movement input
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, moveInput * 2f);
            
            // Draw current velocity
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, currentVelocity);
        }
    }
}