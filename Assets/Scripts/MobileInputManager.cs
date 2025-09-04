using UnityEngine;
using UnityEngine.UI;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance { get; private set; }

    [Header("Touch Settings")]
    public RectTransform touchArea;
    public Image touchIndicator;
    public float touchSensitivity = 0.5f;
    public bool showTouchIndicator = true;

    [Header("Joystick Settings")]
    public MobileJoystick mobileJoystick;
    public bool useJoystick = true;

    [Header("Input State")]
    public bool isTouching = false;
    public Vector2 touchDelta = Vector2.zero;
    public Vector2 touchPosition = Vector2.zero;

    private Vector2 touchStartPos;
    private Vector2 touchCurrentPos;

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
        // Find missing components automatically
        FindMissingComponents();
        
        // Initially hide touch indicator
        if (touchIndicator != null)
        {
            touchIndicator.gameObject.SetActive(false);
        }
        
        // Configure input method based on platform
        ConfigureInputMethod();
        
        // For testing in editor, show joystick if enabled
        if (Application.isEditor && useJoystick && mobileJoystick != null)
        {
            mobileJoystick.gameObject.SetActive(true);
            Debug.Log("Joystick shown for editor testing");
        }
    }
    
    private void FindMissingComponents()
    {
        // Find touch area if not assigned
        if (touchArea == null)
        {
            GameObject touchAreaObj = GameObject.Find("TouchArea");
            if (touchAreaObj != null)
            {
                touchArea = touchAreaObj.GetComponent<RectTransform>();
                Debug.Log("TouchArea found and assigned automatically");
            }
        }
        
        // Find touch indicator if not assigned
        if (touchIndicator == null)
        {
            GameObject touchIndicatorObj = GameObject.Find("TouchIndicator");
            if (touchIndicatorObj != null)
            {
                touchIndicator = touchIndicatorObj.GetComponent<Image>();
                Debug.Log("TouchIndicator found and assigned automatically");
            }
        }
        
        // Find mobile joystick if not assigned
        if (mobileJoystick == null)
        {
            mobileJoystick = FindObjectOfType<MobileJoystick>();
            if (mobileJoystick != null)
            {
                Debug.Log("MobileJoystick found and assigned automatically");
            }
        }
    }

    private void ConfigureInputMethod()
    {
        // Force use joystick on mobile
        if (Application.isMobilePlatform)
        {
            useJoystick = true;
            Debug.Log("Using joystick for mobile platform");
        }
        // Use touch area in editor for testing
        else if (Application.isEditor)
        {
            // For testing in editor, you can choose between joystick and keyboard
            useJoystick = false; // Set to true if you want to test joystick in editor
            Debug.Log("Using keyboard/touch for editor testing");
        }
    }

    void Update()
    {
        if (useJoystick && mobileJoystick != null)
        {
            // Use joystick input
            touchDelta = mobileJoystick.GetInputVector();
            isTouching = touchDelta.magnitude > 0.1f;
        }
        else
        {
            // Use touch area input
            HandleTouchInput();
        }
        
        // Editor input for testing
        HandleEditorInput();
    }

    private void HandleTouchInput()
    {
        // Only handle touch input when game is active
        if (MenuManager.Instance == null || !MenuManager.Instance.isConnected)
        {
            ResetTouchInput();
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Check if touch is within touch area
            if (IsTouchInArea(touch.position))
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchBegan(touch.position);
                        break;

                    case TouchPhase.Moved:
                        OnTouchMoved(touch.position);
                        break;

                    case TouchPhase.Ended:
                        OnTouchEnded();
                        break;

                    case TouchPhase.Canceled:
                        OnTouchEnded();
                        break;
                }
            }
        }
    }

    private void HandleEditorInput()
    {
        // Editor input for testing
        if (Application.isEditor && MenuManager.Instance != null && MenuManager.Instance.isConnected)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                touchDelta = new Vector2(horizontal, vertical);
                isTouching = true;
            }
            else
            {
                touchDelta = Vector2.zero;
                isTouching = false;
            }
        }
    }

    private bool IsTouchInArea(Vector2 touchPos)
    {
        if (touchArea == null) return true;

        // Convert screen position to RectTransform position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            touchArea,
            touchPos,
            null,
            out Vector2 localPoint
        );

        // Check if point is within the touch area
        return touchArea.rect.Contains(localPoint);
    }

    private void OnTouchBegan(Vector2 position)
    {
        touchStartPos = position;
        touchCurrentPos = position;
        isTouching = true;
        touchPosition = position;

        if (showTouchIndicator && touchIndicator != null)
        {
            touchIndicator.gameObject.SetActive(true);
            touchIndicator.transform.position = position;
        }
    }

    private void OnTouchMoved(Vector2 position)
    {
        touchCurrentPos = position;
        touchPosition = position;

        // Calculate delta
        touchDelta = (touchCurrentPos - touchStartPos) * touchSensitivity;

        // Update touch indicator
        if (showTouchIndicator && touchIndicator != null)
        {
            touchIndicator.transform.position = position;
        }
    }

    private void OnTouchEnded()
    {
        isTouching = false;
        touchDelta = Vector2.zero;

        // Hide touch indicator
        if (touchIndicator != null)
        {
            touchIndicator.gameObject.SetActive(false);
        }
    }

    private void ResetTouchInput()
    {
        isTouching = false;
        touchDelta = Vector2.zero;
        touchPosition = Vector2.zero;

        if (touchIndicator != null)
        {
            touchIndicator.gameObject.SetActive(false);
        }
    }

    // Public methods for other scripts to access input
    public Vector2 GetMovementInput()
    {
        return touchDelta.normalized;
    }

    public bool IsMovementActive()
    {
        return isTouching && touchDelta.magnitude > 0.1f;
    }

    public Vector2 GetTouchPosition()
    {
        return touchPosition;
    }

    // For debugging
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            GUILayout.Label("Input Method: " + (useJoystick ? "Joystick" : "Touch Area"));
            GUILayout.Label("Is Touching: " + isTouching);
            GUILayout.Label("Touch Delta: " + touchDelta);
            GUILayout.Label("Touch Position: " + touchPosition);
        }
    }
}