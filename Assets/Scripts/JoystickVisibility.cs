using UnityEngine;

public class JoystickVisibility : MonoBehaviour
{
    private MobileJoystick mobileJoystick;
    private Canvas canvas;

    void Start()
    {
        mobileJoystick = GetComponent<MobileJoystick>();
        canvas = GetComponentInParent<Canvas>();
        
        // Hide joystick initially
        if (mobileJoystick != null)
        {
            mobileJoystick.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (mobileJoystick == null) return;
        
        // Show joystick only when game is active
        bool shouldShow = 
            (MenuManager.Instance != null && MenuManager.Instance.isConnected) ||
            (Application.isEditor);
        
        mobileJoystick.gameObject.SetActive(shouldShow);
    }
}