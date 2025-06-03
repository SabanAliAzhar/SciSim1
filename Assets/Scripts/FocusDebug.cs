using TMPro;
using UnityEngine;

public class FocusDebug : MonoBehaviour
{

    public TMP_InputField[] tmpInputFields = new TMP_InputField[6];

 
    public bool[] isFocused = new bool[6];


    public TMP_InputField focusedTMP;

    void Start()
    {
        
        for (int i = 0; i < tmpInputFields.Length; i++)
        {
            isFocused[i] = false;
            int index = i; // Capture the index for use in the listener
            tmpInputFields[i].onSelect.AddListener((text) => OnSelect(index, text));
            tmpInputFields[i].onDeselect.AddListener((text) => OnDeselect(index, text));
        }
    }


    private void OnSelect(int index, string text)
    {
    
        for (int i = 0; i < isFocused.Length; i++)
        {
            isFocused[i] = (i == index);
        }

        
        focusedTMP = tmpInputFields[index];

        Debug.Log("Input Field " + (index + 1) + " Gained Focus");
    }

    
    private void OnDeselect(int index, string text)
    {
        
        isFocused[index] = false;

        Debug.Log("Input Field " + (index + 1) + " Lost Focus");


    }

  
    public void OnValueChanged(string text)
    {
        if (focusedTMP != null)
        {
            Debug.Log("Current Text of Focused Field (" + focusedTMP.name + "): " + text);
        }
    }
}