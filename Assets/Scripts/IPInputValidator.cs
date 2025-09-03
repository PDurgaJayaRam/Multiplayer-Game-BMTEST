using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

public class IPInputValidator : MonoBehaviour
{
    public TMP_InputField ipInputField;
    public Button joinButton;
    public Image inputFieldImage;
    public Color validColor = new Color(0.8f, 1f, 0.8f, 1f); // Light green
    public Color invalidColor = new Color(1f, 0.8f, 0.8f, 1f); // Light red
    public Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray

    void Start()
    {
        if (ipInputField != null)
        {
            ipInputField.onValueChanged.AddListener(OnInputValueChanged);
            ipInputField.onEndEdit.AddListener(OnInputEndEdit);
        }
    }

    private void OnDestroy()
    {
        if (ipInputField != null)
        {
            ipInputField.onValueChanged.RemoveListener(OnInputValueChanged);
            ipInputField.onEndEdit.RemoveListener(OnInputEndEdit);
        }
    }

    private void OnInputValueChanged(string newValue)
    {
        ValidateIP(newValue);
    }

    private void OnInputEndEdit(string newValue)
    {
        ValidateIP(newValue);
    }

    private void ValidateIP(string ipAddress) // Fixed: Added missing parameter type
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            // Empty input - reset to default color
            SetInputFieldColor(defaultColor);
            EnableJoinButton(false);
            return;
        }

        if (IsValidIPAddress(ipAddress))
        {
            // Valid IP
            SetInputFieldColor(validColor);
            EnableJoinButton(true);
        }
        else
        {
            // Invalid IP
            SetInputFieldColor(invalidColor);
            EnableJoinButton(false);
        }
    }

    private bool IsValidIPAddress(string ipAddress)
    {
        // Check for common IP patterns
        if (Regex.IsMatch(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$"))
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

    private void SetInputFieldColor(Color color)
    {
        if (inputFieldImage != null)
        {
            inputFieldImage.color = color;
        }
    }

    private void EnableJoinButton(bool enable)
    {
        if (joinButton != null)
        {
            joinButton.interactable = enable;
        }
    }
}