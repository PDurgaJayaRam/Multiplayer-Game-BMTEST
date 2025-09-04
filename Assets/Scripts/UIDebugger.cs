using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDebugger : MonoBehaviour
{
    void Start()
    {
        // Check for EventSystem
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("No EventSystem found in scene!");
        }
        else
        {
            Debug.Log("EventSystem found: " + eventSystem.name);
            
            // Check for input modules
            StandaloneInputModule standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule == null)
            {
                Debug.LogError("No StandaloneInputModule found on EventSystem!");
            }
            else
            {
                Debug.Log("StandaloneInputModule found");
            }
            
            // Check for touch input module on mobile
#if UNITY_ANDROID || UNITY_IOS
            TouchInputModule touchInputModule = eventSystem.GetComponent<TouchInputModule>();
            if (touchInputModule == null)
            {
                Debug.LogError("No TouchInputModule found on EventSystem!");
            }
            else
            {
                Debug.Log("TouchInputModule found");
            }
#endif
        }
        
        // Check for Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (canvases.Length == 0)
        {
            Debug.LogError("No Canvas found in scene!");
        }
        else
        {
            Debug.Log($"Found {canvases.Length} Canvas(es)");
            
            foreach (Canvas canvas in canvases)
            {
                Debug.Log($"Canvas: {canvas.name}, Render Mode: {canvas.renderMode}, Sort Order: {canvas.sortingOrder}");
                
                // Check for GraphicRaycaster
                GraphicRaycaster graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster == null)
                {
                    Debug.LogError($"Canvas {canvas.name} has no GraphicRaycaster!");
                }
                else
                {
                    Debug.Log($"Canvas {canvas.name} has GraphicRaycaster");
                }
            }
        }
        
        // Check for buttons
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        if (buttons.Length == 0)
        {
            Debug.LogError("No Buttons found in scene!");
        }
        else
        {
            Debug.Log($"Found {buttons.Length} Button(s)");
            
            foreach (Button button in buttons)
            {
                Debug.Log($"Button: {button.name}, Interactable: {button.interactable}, Raycast Target: {button.targetGraphic.raycastTarget}");
            }
        }
        
        // Check for transparent blocking UI elements
        Image[] images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        int blockingCount = 0;
        
        foreach (Image image in images)
        {
            if (image.color.a == 0 && image.raycastTarget)
            {
                blockingCount++;
                Debug.LogWarning($"Found transparent blocking UI element: {image.gameObject.name}");
            }
        }
        
        if (blockingCount == 0)
        {
            Debug.Log("No transparent blocking UI elements found");
        }
        else
        {
            Debug.LogWarning($"Found {blockingCount} transparent blocking UI element(s)");
        }
    }
}