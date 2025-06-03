using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class ExportLogs : MonoBehaviour
{
    public TMP_Text textDisplay;
    public Button exportButton;

    public enum experiment
    {
        gravity, newotonFirst, Entanglement, projectile
    }

    public experiment currentExpiriment;

    private string fileName;

    void Start()
    {
        fileName = currentExpiriment.ToString() + ".txt";

        if (exportButton == null)
        {
            Debug.LogError("Export button is not assigned. Please assign the button in the inspector.");
            return;
        }

        Debug.Log("File name would be: " + fileName);
        exportButton.onClick.AddListener(OnExportButtonClicked);
    }

    private void OnExportButtonClicked()
    {
        if (textDisplay == null)
        {
            Debug.LogError("Text Display is not assigned. Please assign the TMP_Text in the inspector.");
            return;
        }

        string originalText = textDisplay.text;

        if (string.IsNullOrEmpty(originalText))
        {
            Debug.LogWarning("The text display is empty. There's no text to export.");
            return;
        }

        // Remove HTML tags from the text
        string cleanText = StripHtmlTags(originalText);

        string folderPath = Application.persistentDataPath;
        string fullFilePath = Path.Combine(folderPath, fileName);

        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Compose log with timestamp
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timeStamp}]\n{cleanText}\n\n";

            // Append the log entry
            File.AppendAllText(fullFilePath, logEntry);

            Debug.Log("Text has been successfully appended to: " + fullFilePath);

            // Update TMP text
            textDisplay.text = "Logs have been successfully exported to your storage.";
        }
        catch (IOException ioEx)
        {
            Debug.LogError("I/O error while saving the file: " + ioEx.Message);
            textDisplay.text = "Failed to export logs. Please try again later.";
        }
        catch (UnauthorizedAccessException unauthorizedEx)
        {
            Debug.LogError("Access denied: " + unauthorizedEx.Message);
            textDisplay.text = "You don't have permission to save the file.";
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error: " + ex.Message);
            textDisplay.text = "An unexpected error occurred while exporting logs.";
        }
    }

    private string StripHtmlTags(string input)
    {
        // Remove HTML tags using Regex
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
