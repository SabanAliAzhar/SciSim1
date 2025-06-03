using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Security.Cryptography;
using System.Globalization;

public class GeminiTypeWriter : MonoBehaviour
{
    public float delay = 0.001f;
    public string fullText;
    private Coroutine typingCoroutine;
    void Start()
    {
        
    }

 
    void Update()
    {
        
    }
    public void StartTyping(string text)
    {
        fullText = text;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText());
    }
    private IEnumerator TypeText()
    {
        this.GetComponent<TMP_Text>().text = "";
        foreach(char letter in fullText)
        {
            this.GetComponent<TMP_Text>().text += letter;
            yield return new WaitForSeconds(delay);
        }
    }
    private string AddLineBreaks(string text, int MaxCharacterPerLine)
    {
        string processedText = "";
        int currentLineLength = 0;
        foreach(char c in text)
        {
            processedText += c;
            currentLineLength++;

            if (currentLineLength >= MaxCharacterPerLine && c ==' ')
            {
                processedText += "\n";
                currentLineLength = 0;
            }

        }
        return processedText;
       
    }
}
