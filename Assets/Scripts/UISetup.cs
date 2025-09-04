using UnityEngine;
using UnityEngine.UI;

public class UISetup : MonoBehaviour
{
    void Start()
    {
        SetupJoystickUI();
        SetupTouchArea();
    }
    
    private void SetupJoystickUI()
    {
        // Check if joystick already exists
        if (GameObject.Find("MobileJoystick") != null)
        {
            Debug.Log("MobileJoystick already exists");
            return;
        }
        
        // Find the canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No canvas found!");
            return;
        }
        
        // Create joystick GameObject
        GameObject joystickObj = new GameObject("MobileJoystick");
        joystickObj.transform.SetParent(canvas.transform);
        
        // Add MobileJoystick script
        MobileJoystick joystick = joystickObj.AddComponent<MobileJoystick>();
        
        // Create joystick background
        GameObject backgroundObj = new GameObject("JoystickBackground");
        backgroundObj.transform.SetParent(joystickObj.transform);
        RectTransform backgroundRect = backgroundObj.AddComponent<RectTransform>();
        backgroundRect.sizeDelta = new Vector2(150, 150);
        
        // Position in bottom left corner
        backgroundRect.anchorMin = new Vector2(0, 0);
        backgroundRect.anchorMax = new Vector2(0, 0);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(150, 150); // 150 pixels from bottom left
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(1, 1, 1, 0.3f);
        backgroundImage.raycastTarget = false;
        
        // Create joystick handle
        GameObject handleObj = new GameObject("JoystickHandle");
        handleObj.transform.SetParent(backgroundObj.transform);
        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(60, 60);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        
        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = new Color(1, 1, 1, 0.7f);
        handleImage.raycastTarget = false;
        
        // Assign references
        joystick.joystickBackground = backgroundRect;
        joystick.joystickHandle = handleRect;
        
        // Hide joystick initially
        joystickObj.SetActive(false);
        
        Debug.Log("MobileJoystick created and set up automatically");
    }
    
    private void SetupTouchArea()
    {
        // Check if touch area already exists
        if (GameObject.Find("TouchArea") != null)
        {
            Debug.Log("TouchArea already exists");
            return;
        }
        
        // Create touch area GameObject
        GameObject touchAreaObj = new GameObject("TouchArea");
        touchAreaObj.transform.SetParent(FindFirstObjectByType<Canvas>().transform);
        
        RectTransform touchAreaRect = touchAreaObj.AddComponent<RectTransform>();
        touchAreaRect.anchorMin = Vector2.zero;
        touchAreaRect.anchorMax = Vector2.one;
        touchAreaRect.sizeDelta = Vector2.zero;
        touchAreaRect.anchoredPosition = Vector2.zero;
        
        Image touchAreaImage = touchAreaObj.AddComponent<Image>();
        touchAreaImage.color = new Color(0, 0, 0, 0); // Fully transparent
        touchAreaImage.raycastTarget = false; // Don't block UI
        
        Debug.Log("TouchArea created and set up automatically");
    }
}