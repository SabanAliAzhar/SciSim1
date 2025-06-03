using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class GeminiChatmanager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField playerInputField;
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject newTextPrefab;

    [Header("Gemini API Config")]
    public string apiKey = "AIzaSyCE1dzd8CtBW4dHZLILwNVHAoxYEVRJ21M"; // Never commit this to version control!
    public string apiURL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";   

    void Start()
    {
        StartChat();
    }

    public void StartChat()
    {
        AddToChat("<color=yellow>Sci:</color> How can I help you?");
    }

    public void OnSendButtonClick()
    {
        string playerMessage = playerInputField.text;
        if (string.IsNullOrEmpty(playerMessage)) return;

        AddPlayerChat("<color=yellow>User:</color>" + playerMessage);
        StartCoroutine(SendToGemini(playerMessage));
        playerInputField.text = "";
    }

    private void AddToChat(string message)
    {
        GameObject newText = Instantiate(newTextPrefab, content);
        newText.GetComponent<GeminiTypeWriter>().StartTyping(message);
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
      
        string requestUrl = $"{apiURL}?key={apiKey}";

        string instruction = "You are Sci, a helpful and concise physics assistant. Only answer physics-related questions and reply concisely. If the question is unrelated to physics, respond with: 'I'm here to help with physics-related topics only.'";

        string jsonPayload = "{\"contents\":[{\"parts\":[{\"text\":\"" + instruction + "\\n\\nUser: " + prompt + "\"}]}]}";


        UnityWebRequest request = new UnityWebRequest(requestUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");


        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log(response);

            try
            {
                ChatGPTResponse responseFinal = JsonUtility.FromJson<ChatGPTResponse>(response);
                if (responseFinal.candidates != null && responseFinal.candidates.Length > 0)
                {
                    string text = responseFinal.candidates[0].content.parts[0].text;
                    AddToChat("<color=yellow>Sci:</color> " + text);

                }
                else
                {
                    AddToChat("<color=yellow>Sci:</color> I didn't get a proper response. Please try again.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("JSON Parse Error: " + e.Message);
                AddToChat("Sci: There was an error processing the response.");
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error + "\nResponse: " + request.downloadHandler.text);
            AddToChat("Sci: Sorry, something went wrong. Error: " + request.responseCode);
        }
    }


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