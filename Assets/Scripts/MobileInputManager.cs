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
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Initially hide touch indicator
        if (touchIndicator != null)
        {
            touchIndicator.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        HandleTouchInput();
        HandleEditorInput();
    }
    
    private void HandleTouchInput()
    {
        // Only handle touch input when game is active
        if (!MenuManager.Instance.isConnected)
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
        if (Application.isEditor && MenuManager.Instance.isConnected)
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
        if (showTouchIndicator && touchIndicator != null)
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
            GUILayout.Label("Touch Input Status:");
            GUILayout.Label("Is Touching: " + isTouching);
            GUILayout.Label("Touch Delta: " + touchDelta);
            GUILayout.Label("Touch Position: " + touchPosition);
        }
    }
}