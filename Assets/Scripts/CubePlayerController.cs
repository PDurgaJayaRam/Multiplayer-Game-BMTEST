using Unity.Netcode;
using Unity.Netcode.Components;
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

    [Header("Visual Settings")]
    public Color hostColor = Color.red;
    public Color clientColor = Color.blue;
    public ParticleSystem collisionParticles;

    // Components
    private Rigidbody rb;
    private Renderer playerRenderer;
    private TrailRenderer trailRenderer;
    private NetworkTransform networkTransform;

    // Input state
    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private bool isLocalPlayer;

    // Reference to MenuManager
    private MenuManager menuManager;

    // FIXED: Renamed to avoid conflict with NetworkBehaviour.IsHost
    [HideInInspector]
    public bool isHostPlayer = false;

    // FIXED: Renamed property to avoid conflict
    public bool IsHostPlayer
    {
        get { return isHostPlayer; }
        set { isHostPlayer = value; }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialize components
        InitializeComponents();

        // Set player properties
        SetPlayerProperties();

        // Enable/disable controls based on ownership and connection state
        UpdateControlState();

        // Get MenuManager reference
        menuManager = FindObjectOfType<MenuManager>();
        if (menuManager == null)
        {
            Debug.LogError("MenuManager not found!");
        }
        
        // FIXED: Force enable controls if we're the owner
        if (IsOwner)
        {
            Debug.Log("Forcing control enable for owner");
            EnableControls(true);
        }
    }

    private void InitializeComponents()
    {
        // Get required components
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        networkTransform = GetComponent<NetworkTransform>();

        // Configure rigidbody
        if (rb != null)
        {
            rb.mass = mass;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.05f;
            rb.useGravity = true;
            // FIXED: Don't set isKinematic based on ownership alone
            rb.isKinematic = false;
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
            playerRenderer.material.color = IsHostPlayer ? hostColor : clientColor;
        }

        // Set player name for debugging
        gameObject.name = IsHostPlayer ? "Player_Host" : "Player_Client";

        Debug.Log($"{gameObject.name} spawned. IsOwner: {IsOwner}, IsHostPlayer: {IsHostPlayer}");
    }

    private void UpdateControlState()
    {
        // FIXED: More robust control state logic
        bool controlsEnabled = IsOwner;
        
        // Additional check for MenuManager if it exists
        if (menuManager != null)
        {
            controlsEnabled = controlsEnabled && menuManager.isConnected;
        }

        EnableControls(controlsEnabled);
        
        Debug.Log($"{gameObject.name} controls enabled: {controlsEnabled}");
    }
    
    // NEW METHOD: Centralized control enabling/disabling
    public void EnableControls(bool enabled)
    {
        if (rb != null)
        {
            rb.isKinematic = !enabled;
        }

        if (trailRenderer != null)
        {
            trailRenderer.emitting = enabled;
        }
        
        Debug.Log($"{gameObject.name} controls {(enabled ? "enabled" : "disabled")}");
    }

    void Update()
    {
        // Only handle input for local player
        if (!IsOwner) 
        {
            Debug.Log($"{gameObject.name} is not the owner");
            return;
        }
        
        if (rb != null && rb.isKinematic)
        {
            Debug.Log($"{gameObject.name} rigidbody is kinematic");
        }
        
        HandleInput();
        UpdateVisualEffects();
    }

    void FixedUpdate()
    {
        // Only apply movement for local player
        if (!IsOwner) return;

        ApplyMovement();
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
            
            // Debug log for testing
            if (moveInput.magnitude > 0)
            {
                Debug.Log($"Movement input: {moveInput}");
            }
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
                Debug.Log($"Using keyboard input: {keyboardInput}");
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

    private void UpdateVisualEffects()
    {
        // Update trail renderer based on movement
        if (trailRenderer != null)
        {
            trailRenderer.emitting = currentVelocity.magnitude > 0.5f;
        }
    }

    // Collision handling
    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        // Handle player-to-player collision
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }

        // Handle platform collision
        PlatformController platform = collision.gameObject.GetComponent<PlatformController>();
        if (platform != null)
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
        Vector3 spawnPos = IsHostPlayer ? new Vector3(-3f, 1f, 0f) : new Vector3(3f, 1f, 0f);
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