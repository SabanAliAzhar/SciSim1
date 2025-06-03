using UnityEngine;
// If you need specific UI interactions later, you might add: using UnityEngine.UI;

public class UIPanelController : MonoBehaviour
{
    // --- Assign these in the Unity Inspector ---
    [Header("UI Panels to Control")]
    public GameObject parametersUIPanel;
    public GameObject graphUIPanel;
    public GameObject backgroundUIPanel; // Or rename to theorySupportUIPanel if that's the actual GameObject name
    public GameObject chatBotUIPanel;
    public GameObject vrControlUIPanel;

    // --- Button Action Functions ---

    // Call this function from the button next to "Parameters UI:"
    public void ToggleParametersUI()
    {
        TogglePanel(parametersUIPanel, "Parameters UI");
    }

    // Call this function from the button next to "Graph UI:"
    public void ToggleGraphUI()
    {
        TogglePanel(graphUIPanel, "Graph UI");
    }

    // Call this function from the button next to "Background UI:"
    public void ToggleBackgroundUI() // Rename if needed (e.g., ToggleTheorySupportUI)
    {
        TogglePanel(backgroundUIPanel, "Spatial Panel Manipulator UI Examples (3)"); // Adjust log message if renamed
    }

    // Call this function from the button next to "ChatBot UI:"
    public void ToggleChatBotUI()
    {
        TogglePanel(chatBotUIPanel, "ChatBot UI");
    }

    // Call this function from the button next to "VRcontrol UI:"
    public void ToggleVRControlUI()
    {
        TogglePanel(vrControlUIPanel, "Tutorial Player");
    }

    // --- Helper Function to Toggle Panels ---
    private void TogglePanel(GameObject panel, string panelName)
    {
        if (panel != null)
        {
            // Toggle the active state (if active, make inactive; if inactive, make active)
            bool newState = !panel.activeSelf;
            panel.SetActive(newState);
            Debug.Log(panelName + " panel " + (newState ? "Enabled" : "Disabled"));
        }
        else
        {
            Debug.LogWarning("Attempted to toggle panel '" + panelName + "', but its reference is not set in the UIPanelController script in the Inspector!");
        }
    }

    // --- Optional: Function for the Exit Button ---
    public void ExitApplication()
    {
        Debug.Log("Exit button clicked. Quitting application...");
        // If running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        // If running in a build
#else
        Application.Quit();
#endif
    }

    // --- Optional: Function to Disable All Panels on Start ---
    // void Start()
    // {
    //     // Ensure panels start in a known state (e.g., disabled) if desired
    //     if (parametersUIPanel != null) parametersUIPanel.SetActive(false);
    //     if (graphUIPanel != null) graphUIPanel.SetActive(false);
    //     if (backgroundUIPanel != null) backgroundUIPanel.SetActive(false);
    //     if (chatBotUIPanel != null) chatBotUIPanel.SetActive(false);
    //     if (vrControlUIPanel != null) vrControlUIPanel.SetActive(false);
    //     Debug.Log("All controlled UI Panels initialized to inactive state.");
    // }
}