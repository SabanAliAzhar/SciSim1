using UnityEngine;
using TMPro; // or use UnityEngine.UI for regular InputField

public class OVRKeyboardManager : MonoBehaviour
{
    public TMP_InputField inputField;

    private TouchScreenKeyboard keyboard;

    void Start()
    {
        inputField.onSelect.AddListener(OpenKeyboard);
    }

    void OpenKeyboard(string currentText)
    {
        if (TouchScreenKeyboard.isSupported)
        {
            keyboard = TouchScreenKeyboard.Open(currentText, TouchScreenKeyboardType.Default, false, false, false, false, "Enter text...");
            StartCoroutine(WatchKeyboard());
        }
    }

    System.Collections.IEnumerator WatchKeyboard()
    {
        while (keyboard != null &&
               keyboard.status != TouchScreenKeyboard.Status.Done &&
               keyboard.status != TouchScreenKeyboard.Status.Canceled)
        {
            yield return null;
        }

        if (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            inputField.text = keyboard.text;
        }
    }
}