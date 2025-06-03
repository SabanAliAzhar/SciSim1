
using UnityEngine;
using UnityEngine.UI; // Required for Toggle

public class ReturnToMainMenu : MonoBehaviour
{
    
    public loadScene sceneLoader;

    void Start()
    {
        // Ensure SceneLoader is assigned
        if (sceneLoader == null)
        {
            sceneLoader = FindObjectOfType<loadScene>();
            if (sceneLoader == null)
            {
                Debug.LogError("SceneLoader not found in the scene!");
               
            }
        }
    }

    
    public void loadMainMenu()
    {
        if (sceneLoader != null) sceneLoader.LoadSceneByName("Main");
    }

}
