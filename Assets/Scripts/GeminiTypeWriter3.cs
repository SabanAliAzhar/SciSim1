using UnityEngine;
using TMPro;
using System.Collections;

public class GeminiTypeWriter3 : MonoBehaviour
{
    public float typingSpeed = 0.05f;
    private TMP_Text textComponent;
    private string fullText;
    private Coroutine typingCoroutine;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    public void StartTyping(string text)
    {
        fullText = text;
        textComponent.text = "";
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}