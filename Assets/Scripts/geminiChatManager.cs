using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class geminiChatmanager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField playerInputField;
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject newTextPrefab;

    [Header("Gemini API Config")]
    public string apiKey = "AIzaSyBzRnO9TspDAVkSB9zj0iF6NPLDimFklWY";
    public string apiURL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?";

    void Start()
    {
        StartChat();
    }

    public void StartChat()
    {
        AddToChat("Sci: How can I help you?");
    }

    public void OnSendButtonClick()
    {
        string playerMessage = playerInputField.text;
        if (string.IsNullOrEmpty(playerMessage)) return;

        AddPlayerChat("Player: " + playerMessage);
        StartCoroutine(SendToGemini(playerMessage));
        playerInputField.text = "";
    }

    private void AddToChat(string message)
    {
        GameObject newText = Instantiate(newTextPrefab, content);
        newText.GetComponent<GeminiTypeWriter1>().StartTyping(message);
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    private void AddPlayerChat(string message)
    {
        GameObject newText = Instantiate(newTextPrefab, content);
        newText.GetComponent<TMP_Text>().text = message;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    private IEnumerator SendToGemini(string prompt)
    {
        string jsonPayload = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";
        UnityWebRequest request = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log(response);

            ChatGPTResponse responseFinal = JsonUtility.FromJson<ChatGPTResponse>(response);
            string text = responseFinal.candidates[0].content.parts[0].text;
            AddToChat("Sci: " + text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            AddToChat("Sci: Sorry, something went wrong.");
        }
    }

    // JSON response models
    [System.Serializable]
    public class ChatGPTResponse
    {
        public Candidate[] candidates;
        public UsageMetadata usageMetadata;
    }

    [System.Serializable]
    public class Candidate
    {
        public Content content;
        public string finishReason;
        public float avgLogprobs;
    }

    [System.Serializable]
    public class Content
    {
        public Part[] parts;
        public string role;
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class UsageMetadata
    {
        public int promptTokenCount;
        public int candidatesTokenCount;
        public int totalTokenCount;
    }
}
