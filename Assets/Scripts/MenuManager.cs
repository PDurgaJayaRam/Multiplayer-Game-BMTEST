using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Unity.Netcode;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject touchArea;
    
    [Header("Main Menu Elements")]
    public Button hostButton;
    public Button joinButton;
    public Button searchButton;
    public TMP_InputField ipInputField;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI localIpText;
    public Button quitButton;
    
    [Header("Gameplay Elements")]
    public GameObject gameStatusPanel;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI waitingStatusText; // New text for waiting status
    public TextMeshProUGUI countdownText; // New text for countdown
    public Button backToMenuButton;
    public Button restartButton;
    
    [Header("Connection Settings")]
    public float connectionTimeout = 10f;
    public float statusMessageDuration = 2f;
    public float ipValidationDebounce = 0.5f;
    
    [Header("Game State")]
    public bool isConnected = false;
    public bool isConnecting = false;
    public bool isHosting = false;
    public bool gameStarted = false;
    public bool isWaiting = false; // New state for waiting
    public bool isCountingDown = false; // New state for countdown
    
    [Header("Visual Settings")]
    public Color validIPColor = new Color(0.8f, 1f, 0.8f, 1f);
    public Color invalidIPColor = new Color(1f, 0.8f, 0.8f, 1f);
    public Color defaultIPColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    // Private variables
    private float connectionTimer = 0f;
    private float ipValidationTimer = 0f;
    private string lastStatusMessage = "";
    private Image ipInputFieldImage;
    private Button currentJoinButton;
    private string hostIP = "";
    private Coroutine countdownCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Find missing UI components automatically
        FindMissingComponents();
        
        // Initialize menu system
        InitializeMenu();
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Get and display local IP
        UpdateLocalIP();
        
        // Setup network callbacks
        SetupNetworkCallbacks();
        
        // Setup IP validation
        SetupIPValidation();
    }
    
    private void FindMissingComponents()
    {
        // Find main menu panel if not assigned
        if (mainMenuPanel == null)
        {
            mainMenuPanel = GameObject.Find("MainMenuPanel");
            if (mainMenuPanel == null)
                Debug.LogWarning("mainMenuPanel not found and not assigned!");
        }
        
        // Find gameplay panel if not assigned
        if (gameplayPanel == null)
        {
            gameplayPanel = GameObject.Find("GameplayPanel");
            if (gameplayPanel == null)
                Debug.LogWarning("gameplayPanel not found and not assigned!");
        }
        
        // Find touch area if not assigned
        if (touchArea == null)
        {
            touchArea = GameObject.Find("TouchArea");
            if (touchArea == null)
                Debug.LogWarning("touchArea not found and not assigned!");
        }
        
        // Find game status panel if not assigned
        if (gameStatusPanel == null)
        {
            gameStatusPanel = GameObject.Find("GameStatusPanel");
            if (gameStatusPanel == null)
                Debug.LogWarning("gameStatusPanel not found and not assigned!");
        }
        
        // Find buttons if not assigned
        if (hostButton == null)
        {
            hostButton = GameObject.Find("HostButton")?.GetComponent<Button>();
            if (hostButton == null)
                Debug.LogWarning("hostButton not found and not assigned!");
        }
        
        if (joinButton == null)
        {
            joinButton = GameObject.Find("JoinButton")?.GetComponent<Button>();
            if (joinButton == null)
                Debug.LogWarning("joinButton not found and not assigned!");
        }
        
        if (searchButton == null)
        {
            searchButton = GameObject.Find("SearchButton")?.GetComponent<Button>();
            if (searchButton == null)
                Debug.LogWarning("searchButton not found and not assigned!");
        }
        
        if (quitButton == null)
        {
            quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
            if (quitButton == null)
                Debug.LogWarning("quitButton not found and not assigned!");
        }
        
        if (backToMenuButton == null)
        {
            backToMenuButton = GameObject.Find("BackToMenuButton")?.GetComponent<Button>();
            if (backToMenuButton == null)
                Debug.LogWarning("backToMenuButton not found and not assigned!");
        }
        
        if (restartButton == null)
        {
            restartButton = GameObject.Find("RestartButton")?.GetComponent<Button>();
            if (restartButton == null)
                Debug.LogWarning("restartButton not found and not assigned!");
        }
        
        // Find input fields if not assigned
        if (ipInputField == null)
        {
            ipInputField = GameObject.Find("IPInputField")?.GetComponent<TMP_InputField>();
            if (ipInputField == null)
                Debug.LogWarning("ipInputField not found and not assigned!");
        }
        
        // Find text elements if not assigned
        if (statusText == null)
        {
            statusText = GameObject.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText == null)
                Debug.LogWarning("statusText not found and not assigned!");
        }
        
        if (localIpText == null)
        {
            localIpText = GameObject.Find("LocalIpText")?.GetComponent<TextMeshProUGUI>();
            if (localIpText == null)
                Debug.LogWarning("localIpText not found and not assigned!");
        }
        
        if (gameStatusText == null)
        {
            gameStatusText = GameObject.Find("GameStatusText")?.GetComponent<TextMeshProUGUI>();
            if (gameStatusText == null)
                Debug.LogWarning("gameStatusText not found and not assigned!");
        }
        
        if (waitingStatusText == null)
        {
            waitingStatusText = GameObject.Find("WaitingStatusText")?.GetComponent<TextMeshProUGUI>();
            if (waitingStatusText == null)
                Debug.LogWarning("waitingStatusText not found and not assigned!");
        }
        
        if (countdownText == null)
        {
            countdownText = GameObject.Find("CountdownText")?.GetComponent<TextMeshProUGUI>();
            if (countdownText == null)
                Debug.LogWarning("countdownText not found and not assigned!");
        }
    }
    
    void Update()
    {
        // Handle connection timeout
        if (isConnecting)
        {
            connectionTimer += Time.deltaTime;
            if (connectionTimer >= connectionTimeout)
            {
                HandleConnectionTimeout();
            }
            else
            {
                // Update connection status
                UpdateConnectionStatus();
            }
        }
        
        // Update IP validation timer
        if (ipValidationTimer > 0)
        {
            ipValidationTimer -= Time.deltaTime;
            if (ipValidationTimer <= 0)
            {
                ValidateCurrentIP();
            }
        }
        
        // Update status message timer
        UpdateStatusMessageTimer();
        
        // Handle escape key
        HandleEscapeKey();
    }
    
    private void InitializeMenu()
    {
        // Show main menu by default
        ShowMainMenu();
        
        // Initialize UI state
        SetInteractableState(true);
        ClearStatusMessages();
        
        // Get IP input field image for validation
        if (ipInputField != null)
        {
            ipInputFieldImage = ipInputField.GetComponent<Image>();
            if (ipInputFieldImage == null)
            {
                ipInputFieldImage = ipInputField.gameObject.AddComponent<Image>();
            }
        }
        
        // Get join button reference
        if (joinButton != null)
        {
            currentJoinButton = joinButton;
        }
    }
    
    private void SetupButtonListeners()
    {
        // Main menu buttons
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostButtonClicked);
            
        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinButtonClicked);
            
        if (searchButton != null)
            searchButton.onClick.AddListener(OnSearchButtonClicked);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        
        // Gameplay buttons
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnBackToMenuButtonClicked);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
    }
    
    private void SetupNetworkCallbacks()
    {
        if (CubeNetworkManager.Instance != null)
        {
            var networkManager = CubeNetworkManager.Instance.networkManager;
            
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback += OnClientConnected;
                networkManager.OnClientDisconnectCallback += OnClientDisconnected;
                networkManager.OnServerStarted += OnServerStarted;
                networkManager.OnServerStopped += OnServerStopped;
                networkManager.OnConnectionEvent += OnConnectionEvent;
            }
        }
    }
    
    private void SetupIPValidation()
    {
        if (ipInputField != null)
        {
            ipInputField.onValueChanged.AddListener(OnIPInputValueChanged);
            ipInputField.onEndEdit.AddListener(OnIPInputEndEdit);
        }
    }
    
    #region Button Event Handlers
    
    private void OnHostButtonClicked()
    {
        if (isConnecting || isConnected) 
        {
            UpdateStatus("Already connected or connecting!");
            return;
        }
        
        UpdateStatus("Starting host...");
        SetInteractableState(false);
        isConnecting = true;
        connectionTimer = 0f;
        
        // Start hosting through NetworkManager
        if (CubeNetworkManager.Instance != null)
        {
            CubeNetworkManager.Instance.StartHost();
        }
        else
        {
            UpdateStatus("NetworkManager not found!");
            ResetConnectionState();
        }
    }
    
    private void OnJoinButtonClicked()
    {
        if (isConnecting || isConnected) 
        {
            UpdateStatus("Already connected or connecting!");
            return;
        }
        
        if (ipInputField == null || string.IsNullOrEmpty(ipInputField.text))
        {
            UpdateStatus("Please enter IP address");
            ShakeIPInputField();
            return;
        }
        
        if (!IsValidIPAddress(ipInputField.text))
        {
            UpdateStatus("Invalid IP address format!");
            ShakeIPInputField();
            return;
        }
        
        UpdateStatus($"Connecting to {ipInputField.text}...");
        SetInteractableState(false);
        isConnecting = true;
        connectionTimer = 0f;
        
        // Join game through NetworkManager
        if (CubeNetworkManager.Instance != null)
        {
            CubeNetworkManager.Instance.StartClient(ipInputField.text);
        }
        else
        {
            UpdateStatus("NetworkManager not found!");
            ResetConnectionState();
        }
    }
    
    private void OnSearchButtonClicked()
    {
        // Show common IP ranges and local IP
        UpdateStatus("Searching for games...");
        
        // Show local IP for easy connection
        string localIP = GetLocalIPAddress();
        if (ipInputField != null)
        {
            ipInputField.text = localIP;
        }
        
        // Show additional info
        Invoke(nameof(ShowSearchResults), 1f);
    }
    
    private void OnBackToMenuButtonClicked()
    {
        // Confirm disconnection
        if (isConnected)
        {
            ShowGameStatus("Disconnecting...");
        }
        
        DisconnectFromGame();
        ShowMainMenu();
    }
    
    private void OnRestartButtonClicked()
    {
        ShowGameStatus("Restarting game...");
        RestartGame();
    }
    
    private void OnQuitButtonClicked()
    {
        ShowQuitConfirmation();
    }
    
    #endregion
    
    #region Network Event Handlers
    
    private void OnClientConnected(ulong clientId)
    {
        if (CubeNetworkManager.Instance != null && CubeNetworkManager.Instance.networkManager != null)
        {
            if (clientId == CubeNetworkManager.Instance.networkManager.LocalClientId)
            {
                // This client connected
                UpdateStatus("Connected successfully!");
                isConnecting = false;
                isConnected = true;
                
                // Show gameplay after a short delay
                Invoke(nameof(ShowGameplay), 1f);
                
                Debug.Log($"Client {clientId} connected successfully");
            }
            else
            {
                // Another client connected
                if (isHosting && !isCountingDown && !gameStarted)
                {
                    // Start countdown when another player joins
                    StartCountdown();
                }
                Debug.Log($"Player {clientId} joined the game");
            }
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (CubeNetworkManager.Instance != null && CubeNetworkManager.Instance.networkManager != null)
        {
            if (clientId == CubeNetworkManager.Instance.networkManager.LocalClientId)
            {
                // This client disconnected
                UpdateStatus("Disconnected from server");
                ResetConnectionState();
                ShowMainMenu();
                
                Debug.Log($"Client {clientId} disconnected from server");
            }
            else
            {
                // Another client disconnected
                if (isHosting && gameStarted)
                {
                    ShowGameStatus($"Player {clientId} disconnected");
                }
                Debug.Log($"Player {clientId} left the game");
            }
        }
    }
    
    private void OnServerStarted()
    {
        isHosting = true;
        UpdateStatus("Server started successfully!");
        Debug.Log("Server started");
    }
    
    private void OnServerStopped(bool wasHost)
    {
        isHosting = false;
        UpdateStatus("Server stopped");
        ResetConnectionState();
        Debug.Log($"Server stopped (was host: {wasHost})");
    }
    
    private void OnConnectionEvent(NetworkManager manager, ConnectionEventData eventData)
    {
        Debug.Log($"Connection event: {eventData.EventType} for client {eventData.ClientId}");
    }
    
    #endregion
    
    #region Countdown Methods
    
    private void StartCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        
        isCountingDown = true;
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }
    
    private IEnumerator CountdownCoroutine()
    {
        int count = 10;
        
        while (count > 0)
        {
            // Update countdown text
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
                countdownText.gameObject.SetActive(true);
            }
            
            // Update waiting status
            if (waitingStatusText != null)
            {
                waitingStatusText.text = "Game starting in:";
            }
            
            yield return new WaitForSeconds(1f);
            count--;
        }
        
        // Start the game
        StartGame();
        
        // Hide countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        isCountingDown = false;
    }
    
    #endregion
    
    #region IP Validation
    
    private void OnIPInputValueChanged(string newValue)
    {
        // Debounce validation
        ipValidationTimer = ipValidationDebounce;
    }
    
    private void OnIPInputEndEdit(string newValue)
    {
        // Immediate validation on end edit
        ValidateCurrentIP();
    }
    
    private void ValidateCurrentIP()
    {
        if (ipInputField == null) 
        {
            SetIPInputFieldColor(defaultIPColor);
            EnableJoinButton(false);
            return;
        }
        
        string ipAddress = ipInputField.text;
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            SetIPInputFieldColor(defaultIPColor);
            EnableJoinButton(false);
        }
        else if (IsValidIPAddress(ipAddress))
        {
            SetIPInputFieldColor(validIPColor);
            EnableJoinButton(true);
        }
        else
        {
            SetIPInputFieldColor(invalidIPColor);
            EnableJoinButton(false);
        }
    }
    
    private bool IsValidIPAddress(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress)) return false;
        
        // Check for common IP patterns
        if (System.Text.RegularExpressions.Regex.IsMatch(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$"))
        {
            string[] parts = ipAddress.Split('.');
            if (parts.Length == 4)
            {
                foreach (string part in parts)
                {
                    if (int.TryParse(part, out int number))
                    {
                        if (number < 0 || number > 255)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }
    
    private void SetIPInputFieldColor(Color color)
    {
        if (ipInputFieldImage != null)
        {
            ipInputFieldImage.color = color;
        }
    }
    
    private void EnableJoinButton(bool enable)
    {
        if (currentJoinButton != null)
        {
            currentJoinButton.interactable = enable;
        }
    }
    
    private void ShakeIPInputField()
    {
        if (ipInputField != null)
        {
            StartCoroutine(ShakeAnimation(ipInputField.transform));
        }
    }
    
    private System.Collections.IEnumerator ShakeAnimation(Transform target)
    {
        Vector3 originalPosition = target.localPosition;
        float shakeDuration = 0.5f;
        float shakeMagnitude = 5f;
        
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = originalPosition.y + Random.Range(-shakeMagnitude, shakeMagnitude);
            
            target.localPosition = new Vector3(x, y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            
            yield return null;
        }
        
        target.localPosition = originalPosition;
    }
    
    #endregion
    
    #region Menu State Management
    
    public void ShowMainMenu()
    {
        // Show main menu
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
            
        // Hide gameplay elements
        if (gameplayPanel != null)
            gameplayPanel.SetActive(false);
            
        if (gameStatusPanel != null)
            gameStatusPanel.SetActive(false);
            
        if (touchArea != null)
            touchArea.SetActive(false);
            
        // Reset game state
        isConnected = false;
        isConnecting = false;
        isHosting = false;
        gameStarted = false;
        isWaiting = false;
        isCountingDown = false;
        
        // Enable menu interactions
        SetInteractableState(true);
        
        // Update local IP
        UpdateLocalIP();
        
        // Reset IP validation
        ValidateCurrentIP();
        
        Debug.Log("Main menu shown");
    }
    
    public void ShowGameplay()
    {
        // Hide main menu
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        // Show gameplay elements
        if (gameplayPanel != null)
            gameplayPanel.SetActive(true);
            
        if (gameStatusPanel != null)
            gameStatusPanel.SetActive(true);
            
        if (touchArea != null)
            touchArea.SetActive(true);
            
        // Update game state
        isConnected = true;
        isConnecting = false;
        
        if (isHosting)
        {
            // Host is waiting for players
            isWaiting = true;
            hostIP = GetLocalIPAddress();
            
            // Show waiting status with IP
            if (waitingStatusText != null)
            {
                waitingStatusText.text = $"Waiting for players...\nHost IP: {hostIP}";
                waitingStatusText.gameObject.SetActive(true);
            }
            
            // Hide countdown text initially
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Client joined the game
            isWaiting = false;
            
            // Hide waiting status
            if (waitingStatusText != null)
            {
                waitingStatusText.gameObject.SetActive(false);
            }
            
            // Hide countdown text
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }
        
        Debug.Log("Gameplay shown");
    }
    
    #endregion
    
    #region UI Management
    
    public void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            lastStatusMessage = message;
            Debug.Log($"Status updated: {message}");
        }
    }
    
    public void UpdateLocalIP()
    {
        if (localIpText != null)
        {
            string localIP = GetLocalIPAddress();
            localIpText.text = $"Your IP: {localIP}";
            Debug.Log($"Local IP updated: {localIP}");
        }
    }
    
    public void ShowGameStatus(string message)
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = message;
            Debug.Log($"Game status: {message}");
            
            // Auto-clear status message after duration
            Invoke(nameof(ClearGameStatus), statusMessageDuration);
        }
    }
    
    private void ClearGameStatus()
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = "";
        }
    }
    
    private void ClearStatusMessages()
    {
        if (statusText != null)
            statusText.text = "Ready";
            
        if (gameStatusText != null)
            gameStatusText.text = "";
            
        if (waitingStatusText != null)
            waitingStatusText.text = "";
            
        if (countdownText != null)
            countdownText.text = "";
    }
    
    private void SetInteractableState(bool interactable)
    {
        // Enable/disable main menu buttons
        if (hostButton != null)
            hostButton.interactable = interactable;
            
        if (joinButton != null)
            joinButton.interactable = interactable && (ipInputField != null ? IsValidIPAddress(ipInputField.text) : false);
            
        if (searchButton != null)
            searchButton.interactable = interactable;
            
        if (ipInputField != null)
            ipInputField.interactable = interactable;
        
        if (quitButton != null)
            quitButton.interactable = interactable;
            
        // Enable/disable gameplay buttons
        if (backToMenuButton != null)
            backToMenuButton.interactable = !interactable;
            
        if (restartButton != null)
            restartButton.interactable = !interactable;
    }
    
    private void ShowSearchResults()
    {
        string localIP = GetLocalIPAddress();
        UpdateStatus($"Use this IP: {localIP}");
        
        // Show additional helpful information
        Invoke(nameof(ShowConnectionTips), 2f);
    }
    
    private void ShowConnectionTips()
    {
        UpdateStatus("Make sure both devices are on the same WiFi network");
    }
    
    private void UpdateConnectionStatus()
    {
        if (isConnecting)
        {
            float timeRemaining = connectionTimeout - connectionTimer;
            UpdateStatus($"Connecting... {Mathf.Ceil(timeRemaining)}s");
        }
    }
    
    private void UpdateStatusMessageTimer()
    {
        // Auto-clear connection status messages
        if (isConnecting && connectionTimer > 3f)
        {
            // Don't clear, just show elapsed time
            // This is handled in UpdateConnectionStatus
        }
    }
    
    #endregion
    
    #region Game Management
    
    public void StartGame()
    {
        if (isHosting)
        {
            // Notify all clients that the game is starting
            if (CubeNetworkManager.Instance != null)
            {
                CubeNetworkManager.Instance.StartGame();
            }
        }
        
        // Update game state
        gameStarted = true;
        isWaiting = false;
        isCountingDown = false;
        
        // Hide waiting status
        if (waitingStatusText != null)
        {
            waitingStatusText.gameObject.SetActive(false);
        }
        
        // Hide countdown text
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        // Show game started message
        ShowGameStatus("Game started!");
        
        Debug.Log("Game started");
    }
    
    private void RestartGame()
    {
        // Reset all players
        CubePlayerController[] players = FindObjectsOfType<CubePlayerController>();
        foreach (var player in players)
        {
            player.ResetPlayer();
        }
        
        ShowGameStatus("Game restarted!");
        
        Debug.Log("Game restarted");
    }
    
    private void ShowQuitConfirmation()
    {
        ShowGameStatus("Press again to quit");
        Invoke(nameof(QuitGame), 2f);
    }
    
    private void QuitGame()
    {
        Debug.Log("Quitting game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Connection Management
    
    private void DisconnectFromGame()
    {
        if (CubeNetworkManager.Instance != null)
        {
            CubeNetworkManager.Instance.networkManager.Shutdown();
        }
        
        ResetConnectionState();
        UpdateStatus("Disconnected");
    }
    
    private void ResetConnectionState()
    {
        isConnecting = false;
        isConnected = false;
        isHosting = false;
        gameStarted = false;
        isWaiting = false;
        isCountingDown = false;
        connectionTimer = 0f;
        
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        
        SetInteractableState(true);
    }
    
    private void HandleConnectionTimeout()
    {
        UpdateStatus("Connection timeout! Please try again.");
        ResetConnectionState();
        ShakeIPInputField();
    }
    
    #endregion
    
    #region Input Handling
    
    private void HandleEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isConnected)
            {
                OnBackToMenuButtonClicked();
            }
        }
    }
    
    #endregion
    
    #region Utility Methods
    
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
    
    // Public methods for external calls
    public void PlayerFellOffPlatform(string playerName)
    {
        if (gameStarted)
        {
            ShowGameStatus($"{playerName} fell off!");
        }
    }
    
    public void PlayerScored(string playerName, int points)
    {
        if (gameStarted)
        {
            ShowGameStatus($"{playerName} scored! +{points}");
        }
    }
    
    public void GameOver(string winner)
    {
        if (gameStarted)
        {
            ShowGameStatus($"Game Over! {winner} wins!");
        }
    }
    
    public void OnPlayerSpawned(GameObject player)
    {
        if (isConnected)
        {
            string playerType = player.GetComponent<CubePlayerController>().IsHost ? "Host" : "Client";
            
            if (gameStarted)
            {
                ShowGameStatus($"{playerType} player spawned!");
            }
            else if (isWaiting)
            {
                // Player spawned while waiting
                Debug.Log($"{playerType} player spawned in waiting state");
            }
        }
    }
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        // Remove network callbacks
        if (CubeNetworkManager.Instance != null)
        {
            var networkManager = CubeNetworkManager.Instance.networkManager;
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
                networkManager.OnServerStarted -= OnServerStarted;
                networkManager.OnServerStopped -= OnServerStopped;
                networkManager.OnConnectionEvent -= OnConnectionEvent;
            }
        }
        
        // Remove IP validation listeners
        if (ipInputField != null)
        {
            ipInputField.onValueChanged.RemoveListener(OnIPInputValueChanged);
            ipInputField.onEndEdit.RemoveListener(OnIPInputEndEdit);
        }
    }
    
    #endregion
}