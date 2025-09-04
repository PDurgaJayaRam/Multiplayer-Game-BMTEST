using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Joystick Settings")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    public float handleRange = 1f;
    public float deadZone = 0.1f;

    [Header("Input Settings")]
    public bool normalizeOutput = true;
    public float sensitivity = 1f;

    private Vector2 inputVector = Vector2.zero;
    private Vector2 joystickOriginalPosition;
    private RectTransform joystickBaseRectTransform;

    private void Start()
    {
        // Find missing components automatically
        if (joystickBackground == null)
        {
            GameObject backgroundObj = GameObject.Find("JoystickBackground");
            if (backgroundObj != null)
            {
                joystickBackground = backgroundObj.GetComponent<RectTransform>();
                Debug.Log("JoystickBackground found and assigned automatically");
            }
            else
            {
                Debug.LogError("JoystickBackground not found! Please assign it in the inspector or create a GameObject named 'JoystickBackground'");
            }
        }
        
        if (joystickHandle == null)
        {
            GameObject handleObj = GameObject.Find("JoystickHandle");
            if (handleObj != null)
            {
                joystickHandle = handleObj.GetComponent<RectTransform>();
                Debug.Log("JoystickHandle found and assigned automatically");
            }
            else
            {
                Debug.LogError("JoystickHandle not found! Please assign it in the inspector or create a GameObject named 'JoystickHandle'");
            }
        }
        
        joystickBaseRectTransform = GetComponent<RectTransform>();
        
        // Store original position and hide joystick initially
        if (joystickBackground != null)
        {
            joystickOriginalPosition = joystickBackground.position;
            joystickBackground.gameObject.SetActive(false);
        }
        
        if (joystickHandle != null)
        {
            joystickHandle.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if references are valid
        if (joystickBackground == null || joystickHandle == null)
        {
            Debug.LogError("Joystick references not set!");
            return;
        }
        
        // Move joystick to touch position
        joystickBackground.position = eventData.position;
        joystickBackground.gameObject.SetActive(true);
        joystickHandle.gameObject.SetActive(true);
        
        // Reset handle position
        joystickHandle.localPosition = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Check if references are valid
        if (joystickBackground == null || joystickHandle == null)
        {
            return;
        }
        
        // Get joystick background rect
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 position
        );

        // Calculate distance from center
        float distance = position.magnitude;
        
        // Normalize if outside range
        if (distance > handleRange)
        {
            position = position.normalized * handleRange;
        }
        
        // Set handle position
        joystickHandle.localPosition = position;
        
        // Calculate input vector
        inputVector = position / handleRange;
        
        // Apply dead zone
        if (inputVector.magnitude < deadZone)
        {
            inputVector = Vector2.zero;
        }
        else if (normalizeOutput)
        {
            inputVector = inputVector.normalized;
        }
        
        // Apply sensitivity
        inputVector *= sensitivity;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Check if references are valid
        if (joystickBackground == null || joystickHandle == null)
        {
            return;
        }
        
        // Reset joystick
        joystickBackground.position = joystickOriginalPosition;
        joystickBackground.gameObject.SetActive(false);
        joystickHandle.gameObject.SetActive(false);
        inputVector = Vector2.zero;
    }

    public Vector2 GetInputVector()
    {
        return inputVector;
    }

    public float GetHorizontal()
    {
        return inputVector.x;
    }

    public float GetVertical()
    {
        return inputVector.y;
    }
}