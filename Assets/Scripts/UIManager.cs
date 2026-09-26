using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<UIWindow> _uiWindows;
    public List<UIWindow> UIWindows => _uiWindows;

    public void ShowWindow(string windowName)
    {
        foreach (var window in _uiWindows)
        {
            if (window.Id == windowName)
            {
                Debug.Log($"Showing window: {windowName}");
                window.Show();
                break;
            }
            else
            {
                Debug.LogError("Window not found: " + windowName);
            }
        }
    }

    public void HideWindow(string windowName)
    {
        foreach (var window in _uiWindows)
        {
            if (window.Id == windowName)
            {
                Debug.Log($"Hiding window: {windowName}");
                window.Hide();
                break;
            }
            else
            {
                Debug.LogError("Window not found: " + windowName);
            }
        }
    }
}
