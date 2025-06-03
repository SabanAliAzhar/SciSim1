using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrateTextScript : MonoBehaviour
{
    [Header("UI Elements")]
    public Button narrateButton;
    public Button openLinkButton;
    public TextMeshProUGUI buttonText;

    [Header("Audio")]
    public AudioClip narrationClip;
    private AudioSource audioSource;

    private bool isNarrating = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (narrationClip != null)
        {
            audioSource.clip = narrationClip;
        }

        if (narrateButton != null)
        {
            narrateButton.onClick.AddListener(OnNarrateButtonClick);
        }

        if (openLinkButton != null)
        {
            openLinkButton.onClick.AddListener(OpenWikipediaPage);
        }

        if (buttonText != null)
        {
            buttonText.text = "Play Narration";
        }
    }

    void OnNarrateButtonClick()
    {
        if (isNarrating)
        {
            StopNarration();
        }
        else
        {
            StartNarration();
        }
    }

    void StartNarration()
    {
        if (!isNarrating && narrationClip != null && audioSource != null)
        {
            audioSource.Play();
            isNarrating = true;
            if (buttonText != null)
            {
                buttonText.text = "Stop Narration";
            }
        }
        else
        {
            Debug.Log("Narration clip is missing or already playing.");
        }
    }

    void StopNarration()
    {
        if (isNarrating && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            isNarrating = false;
            if (buttonText != null)
            {
                buttonText.text = "Play Narration";
            }
        }
    }

    void OpenWikipediaPage()
    {
        Application.OpenURL("https://en.wikipedia.org/wiki/Gravity_of_Earth");
    }
}
