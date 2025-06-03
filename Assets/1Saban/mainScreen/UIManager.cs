using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject simulationsDisplayPanel;
    public GameObject quizzesDisplayPanel;

    [Header("Mode Toggles")]
    public Toggle simulationToggle;
    public Toggle quizToggle;

 
    public SceneLoader sceneLoader;

  
    void Start()
    {
        // Ensure SceneLoader is assigned
        if (sceneLoader == null)
        {
            sceneLoader = FindObjectOfType<SceneLoader>();
            if (sceneLoader == null)
            {
                Debug.LogError("SceneLoader not found in the scene!");
              
            }
        }


 
        if (simulationToggle != null)
        {
            simulationToggle.onValueChanged.AddListener(OnSimulationToggleChanged);
        }
        if (quizToggle != null)
        {
            quizToggle.onValueChanged.AddListener(OnQuizToggleChanged);
        }

   
        if (simulationsDisplayPanel != null) simulationsDisplayPanel.SetActive(true);
        if (quizzesDisplayPanel != null) quizzesDisplayPanel.SetActive(false);
        if (simulationToggle != null) simulationToggle.isOn = true; 
    }

    void OnSimulationToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (simulationsDisplayPanel != null) simulationsDisplayPanel.SetActive(true);
            if (quizzesDisplayPanel != null) quizzesDisplayPanel.SetActive(false);
        }
    }

    void OnQuizToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (simulationsDisplayPanel != null) simulationsDisplayPanel.SetActive(false);
            if (quizzesDisplayPanel != null) quizzesDisplayPanel.SetActive(true);
           
        }
    }

   

    public void LoadNewtonScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
  
}