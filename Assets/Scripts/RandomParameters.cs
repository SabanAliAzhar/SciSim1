using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomParameters : MonoBehaviour
{
    public TMP_InputField massInput;
    public TMP_InputField heightInput;
    public TMP_Dropdown dropdown;
    public Toggle optionToggle;

    public void RandomParameterization()
    {
      
        int mass = Random.Range(5, 16);
        int height;
        do
        {
            height = Random.Range(5, 16);
        } while (height == mass);

   
        if (massInput != null)
            massInput.text = mass.ToString();
        else
            Debug.LogWarning("Mass InputField is not assigned!");

        if (heightInput != null)
            heightInput.text = height.ToString();
        else
            Debug.LogWarning("Height InputField is not assigned!");

       
        if (dropdown != null && dropdown.options.Count >= 4)
        {
            int dropdownIndex = Random.Range(1, 4);
            dropdown.value = dropdownIndex;
            dropdown.RefreshShownValue();
            Debug.Log($"Dropdown set to: {dropdown.options[dropdownIndex].text}");
        }
        else
        {
            Debug.LogWarning("Dropdown is not assigned or doesn't have at least 4 options!");
        }


        if (optionToggle != null)
        {
            bool toggleState = Random.value > 0.5f;
            optionToggle.isOn = toggleState;
            Debug.Log("Toggle set to: " + toggleState);
        }
        else
        {
            Debug.LogWarning("Toggle is not assigned!");
        }

        Debug.Log($"Mass={mass}, Height={height}");
    }
}