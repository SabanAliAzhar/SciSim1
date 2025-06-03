using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Linq;

public class GravityExperiment : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject footballObject;
    public Transform groundPlane;

    [Header("UI Elements")]
    public TextMeshProUGUI statsText;
    public TMP_InputField massInputField;
    public TMP_InputField heightInputField;
    public TextMeshProUGUI errorText;
    public Button setParametersButton;
    public Button dropButton;
    public TMP_Dropdown gravityDropdown;
    public Toggle airResistanceToggle;
    public GameObject airResistancePanel;

    [Header("Settings")]
    public float gravity = 9.81f;
    public float groundThreshold = 0.01f;

    [Header("Air Resistance")]
    public bool useAirResistance = false;
    public float dragCoefficient = 0.47f;
    public float airDensity = 1.225f;
    public float objectCrossSectionalArea = 0.038f;

    private const float EARTH_GRAVITY = 9.81f;
    private const float MOON_GRAVITY = 1.62f;
    private const float MARS_GRAVITY = 3.71f;

    // Air resistance constants
    private const float EARTH_DRAG_COEFFICIENT = 0.47f;
    private const float EARTH_AIR_DENSITY = 1.225f;
    private const float MARS_DRAG_COEFFICIENT = 0.42f;
    private const float MARS_AIR_DENSITY = 0.020f;
    private const float MOON_DRAG_COEFFICIENT = 0f;
    private const float MOON_AIR_DENSITY = 0f;

    private Rigidbody footballRb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    public float elapsedSinceDrop = 0f; 
    public float initialHeight = 0f;
    public float finalVelocity = 0f;    
    public float avgVelocity = 0f;

    private bool isHeld = false;
    private bool isFalling = false;
    private bool hasLanded = false;
    private bool parametersSet = false;
    private float expectedFallDuration = 0f;
    private float currentMass = 1f;

    private float landedTimeStorage;
    private float landedVelocityStorage;


    private float liveVelocityDuringFall;


    void Start()
    {
        footballRb = footballObject.GetComponent<Rigidbody>();
        grabInteractable = footballObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        setParametersButton.onClick.AddListener(SetParameters);
        dropButton.onClick.AddListener(DropFromUI);
        airResistanceToggle.onValueChanged.AddListener(OnAirResistanceToggleChanged);

        errorText.text = "";
        statsText.text = "Status: Ready";

        InitializeGravityDropdown();
    }

    private void OnAirResistanceToggleChanged(bool isOn)
    {
        useAirResistance = isOn;
    }

    void InitializeGravityDropdown()
    {
        gravityDropdown.onValueChanged.RemoveAllListeners();
        gravityDropdown.ClearOptions();

        List<string> options = new List<string> { "Earth", "Moon", "Mars" };
        gravityDropdown.AddOptions(options);
        gravityDropdown.value = 0;
        gravity = EARTH_GRAVITY; // Default to Earth
        UpdatePhysicsGravity();

        gravityDropdown.onValueChanged.AddListener(OnGravityChanged);
    }

    void Update()
    {
        if (isFalling && !hasLanded)
        {
            elapsedSinceDrop += Time.deltaTime;

            float currentHeight = Mathf.Max(footballObject.transform.position.y - groundPlane.position.y, 0f);
            // Calculate live velocity
            float localCurrentVelocity = Mathf.Sqrt(2 * gravity * Mathf.Max(initialHeight - currentHeight, 0f));
            liveVelocityDuringFall = localCurrentVelocity; // Store it for the getter

            statsText.text = $"<b>[FALLING]</b>\n" +
                             $"Initial Height: <color=yellow>{initialHeight:F2}</color> m\n" +
                             $"Elapsed Time: <color=yellow>{elapsedSinceDrop:F2}</color> s\n" +
                             $"Estimated Velocity: <color=yellow>{liveVelocityDuringFall:F2}</color> m/s\n" + // Use the stored live velocity
                             $"Remaining Height: <color=yellow>{currentHeight:F2}</color> m";
        }
        else if (!isFalling) 
        {
            liveVelocityDuringFall = 0f; 
        }
    }

    private void OnGravityChanged(int index)
    {
        switch (index)
        {
            case 0: // Earth
                gravity = EARTH_GRAVITY;
                dragCoefficient = EARTH_DRAG_COEFFICIENT;
                airDensity = EARTH_AIR_DENSITY;
                airResistancePanel.SetActive(true);
                break;
            case 1: // Moon
                gravity = MOON_GRAVITY;
                dragCoefficient = MOON_DRAG_COEFFICIENT;
                airDensity = MOON_AIR_DENSITY;
                airResistancePanel.SetActive(false);
                useAirResistance = false;
                airResistanceToggle.isOn = false;
                break;
            case 2: // Mars
                gravity = MARS_GRAVITY;
                dragCoefficient = MARS_DRAG_COEFFICIENT;
                airDensity = MARS_AIR_DENSITY;
                airResistancePanel.SetActive(true);
                break;
        }
        UpdatePhysicsGravity();
    }

    private void UpdatePhysicsGravity()
    {
        if (footballRb != null)
        {
           
            Physics.gravity = new Vector3(0, -gravity, 0);
            footballRb.useGravity = true;
            footballRb.WakeUp(); 
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        ResetSimulation();
        isHeld = true;
        footballRb.isKinematic = true;
        footballRb.linearVelocity = Vector3.zero;
        footballRb.angularVelocity = Vector3.zero;
        statsText.text = "<b>[GRAB]</b>\nBall grabbed.";
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (!parametersSet)
        {
           
            initialHeight = Mathf.Max(0.1f, footballObject.transform.position.y - groundPlane.position.y); 
          
            Debug.LogWarning("Parameters not set, using release height: " + initialHeight);
        }


        isHeld = false;
        isFalling = true;
        hasLanded = false;
        elapsedSinceDrop = 0f; 
        liveVelocityDuringFall = 0f; 

        footballRb.isKinematic = false;
      
        footballRb.linearVelocity = Vector3.zero;
        footballRb.angularVelocity = Vector3.zero;

        statsText.text = "<b>[RELEASE]</b>\nBall released.";
    }

    private void SetParameters()
    {
        errorText.text = "";

        if (string.IsNullOrWhiteSpace(massInputField.text) || string.IsNullOrWhiteSpace(heightInputField.text))
        {
            errorText.text = "Please enter both Mass and Height.";
            parametersSet = false;
            return;
        }

        if (!float.TryParse(massInputField.text, out float mass) ||
            !float.TryParse(heightInputField.text, out float height))
        {
            errorText.text = "Invalid numeric values!";
            parametersSet = false;
            return;
        }

        if (mass <= 0 || height <= 0)
        {
            errorText.text = "Mass and height must be greater than 0.";
            parametersSet = false;
            return;
        }

        footballRb.mass = mass;
        currentMass = mass;
       

        footballObject.transform.position = new Vector3(
            footballObject.transform.position.x,
            groundPlane.position.y + height,
            footballObject.transform.position.z
        );

        initialHeight = height;
 

        parametersSet = true;
        ResetSimulation(); 
        footballRb.isKinematic = true; 

        statsText.text = $"<b>[SET]</b>\nMass = <color=yellow>{mass:F2}</color> kg\nHeight = <color=yellow>{height:F2}</color> m\nGravity = <color=yellow>{gravity:F2}</color> m/s²";
    }

    private void DropFromUI()
    {
        if (!parametersSet)
        {
            errorText.text = "Please set parameters before dropping.";
            return;
        }

        ResetSimulation(); 

        isFalling = true;
        hasLanded = false; 
        elapsedSinceDrop = 0f; 
        liveVelocityDuringFall = 0f;

        // Position is already set by SetParameters
        footballRb.isKinematic = false;
        footballRb.linearVelocity = Vector3.zero;
        footballRb.angularVelocity = Vector3.zero;

        statsText.text = "<b>[DROP]</b>\nBall dropped using set parameters.";
    }

    private void ShowLandingStats()
    {
        float calculatedFallTime; 
        float calculatedFinalVelocity; 

        if (!useAirResistance)
        {
            calculatedFinalVelocity = Mathf.Sqrt(2 * gravity * initialHeight);
            avgVelocity = calculatedFinalVelocity / 2f; 
            calculatedFallTime = Mathf.Sqrt((2 * initialHeight) / gravity);

            statsText.text = $"<b>[LANDED]</b>\n" +
                             $"Drop Height: <color=yellow>{initialHeight:F2}</color> m\n" +
                             $"Final Velocity: <color=yellow>{calculatedFinalVelocity:F2}</color> m/s\n" +
                             $"Average Velocity: <color=yellow>{avgVelocity:F2}</color> m/s\n" +
                             $"Fall Duration: <color=yellow>{calculatedFallTime:F2}</color> s\n" +
                             $"Gravity: <color=yellow>{gravity:F2}</color> m/s²";
        }
        else
        {
            float vt = Mathf.Sqrt((2 * currentMass * gravity) / (airDensity * dragCoefficient * objectCrossSectionalArea));
     
            float t0_ideal = Mathf.Sqrt((2 * initialHeight) / gravity);
           
            calculatedFallTime = elapsedSinceDrop; 
            calculatedFinalVelocity = vt * (float)System.Math.Tanh((gravity * calculatedFallTime) / vt); 
            avgVelocity = (calculatedFallTime > 0) ? initialHeight / calculatedFallTime : 0;


            statsText.text = $"<b>[LANDED - AIR RESIST]</b>\n" +
                             $"Drop Height: <color=yellow>{initialHeight:F2}</color> m\n" +
                             $"Final Velocity (approx): <color=yellow>{calculatedFinalVelocity:F2}</color> m/s\n" +
                             $"Average Velocity: <color=yellow>{avgVelocity:F2}</color> m/s\n" +
                             $"Fall Duration (measured): <color=yellow>{calculatedFallTime:F2}</color> s\n" + 
                             $"Terminal Velocity: <color=yellow>{vt:F2}</color> m/s\n" +
                             $"Gravity: <color=yellow>{gravity:F2}</color> m/s²";
        }

        
        landedTimeStorage = calculatedFallTime;
        landedVelocityStorage = calculatedFinalVelocity;
       
        this.finalVelocity = calculatedFinalVelocity;


        Debug.Log(statsText.text);
    }

   
    public float GetCurrentTime()
    {
        if (isFalling)
        {
            return elapsedSinceDrop; 
        }
        else
        {
            return landedTimeStorage; 
        }
    }

    public float GetCurrentVelocity()
    {
        if (isFalling)
        {
            return liveVelocityDuringFall; 
        }
        else
        {
            return landedVelocityStorage; 
        }
    }

    public bool GetFallStatus() => isFalling;


    private void ResetSimulation()
    {
        elapsedSinceDrop = 0f;
        liveVelocityDuringFall = 0f; 
       
        this.finalVelocity = 0f; 
        avgVelocity = 0f;
        isFalling = false;
        hasLanded = false;
    }

    public void HandleCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && isFalling && !hasLanded)
        {
           

            isFalling = false;
            hasLanded = true;

            footballRb.isKinematic = true;
            footballRb.linearVelocity = Vector3.zero;
            footballRb.angularVelocity = Vector3.zero;

            ShowLandingStats(); 
        }
    }
}