using UnityEngine;
using UnityEngine.UI; // Required for UI elements
using TMPro; // Required for TextMeshPro

public class PhysicsDemoManager : MonoBehaviour
{
    [Header("Object to Control")]
    public Rigidbody objectRigidbody;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [Header("UI Elements")]
    public Slider gravitySlider;
    public TMP_Text gravityValueText;

    public Slider massSlider;
    public TMP_Text massValueText;

    public Slider forceSlider;
    public TMP_Text forceValueText;

    public Button applyForceButton;
    public Button resetButton;

    public DistanceTimeGraph distanceTimeGraphController;
    // Internal state variables
    private float currentGravityY = -9.81f;
    private float currentMass = 1f;
    private float currentForceMagnitude = 10f;

    void Start()
    {
        if (!objectRigidbody)
        {
            Debug.LogError("Object Rigidbody not assigned!");
            enabled = false; // Disable script if no object
            return;
        }

        // Store initial state of the object for reset
        initialPosition = objectRigidbody.transform.position;
        initialRotation = objectRigidbody.transform.rotation;

        // --- Setup UI Listeners & Initial Values ---

        // Gravity
        if (gravitySlider)
        {
            gravitySlider.minValue = -20f;
            gravitySlider.maxValue = 0f;
            gravitySlider.value = currentGravityY;
            gravitySlider.onValueChanged.AddListener(SetGravity);
            SetGravity(gravitySlider.value); // Initial call
        }

        // Mass
        if (massSlider)
        {
            massSlider.minValue = 0.1f;
            massSlider.maxValue = 10f;
            massSlider.value = currentMass;
            massSlider.onValueChanged.AddListener(SetMass);
            SetMass(massSlider.value); // Initial call
        }

        // Force
        if (forceSlider)
        {
            forceSlider.minValue = 0f;
            forceSlider.maxValue = 100f;
            forceSlider.value = currentForceMagnitude;
            forceSlider.onValueChanged.AddListener(SetForceMagnitude);
            SetForceMagnitude(forceSlider.value); // Initial call
        }

        // Buttons
        if (applyForceButton)
        {
            applyForceButton.onClick.AddListener(ApplyForceToObject);
        }
        if (resetButton)
        {
            resetButton.onClick.AddListener(ResetObject);
        }

        // Initial physics setup
        ApplyPhysicsSettings();
    }

    void ApplyPhysicsSettings()
    {
        Physics.gravity = new Vector3(0, currentGravityY, 0);
        if (objectRigidbody)
        {
            objectRigidbody.mass = currentMass;
        }
    }

    public void SetGravity(float value)
    {
        currentGravityY = value;
        Physics.gravity = new Vector3(0, currentGravityY, 0);
        if (gravityValueText) gravityValueText.text = value.ToString("F2");
    }

    public void SetMass(float value)
    {
        currentMass = value;
        if (objectRigidbody) objectRigidbody.mass = currentMass;
        if (massValueText) massValueText.text = value.ToString("F2") + " kg";
    }

    public void SetForceMagnitude(float value)
    {
        currentForceMagnitude = value;
        if (forceValueText) forceValueText.text = value.ToString("F0") + " N";
    }


    // In PhysicsDemoManager.cs

    // (Ensure you have: public DistanceTimeGraph distanceTimeGraphController;)

    public void ApplyForceToObject()
    {
        if (!objectRigidbody) return;

        Vector3 forceDirection = objectRigidbody.transform.forward;
        objectRigidbody.AddForce(forceDirection * currentForceMagnitude, ForceMode.Impulse);
        Debug.Log($"Applied force: {currentForceMagnitude}N in direction {forceDirection}");

        // --- ADD THIS ---
        // Automatically start the graph when force is applied, if it's not already running
        if (distanceTimeGraphController != null && !distanceTimeGraphController.IsGraphRecording())
        {
            distanceTimeGraphController.StartRecording();
        }
        // ---------------
    }

    // ResetObject() method already has distanceTimeGraphController.StopAndClearGraph();
    // which is good.

    public void ResetObject()
    {
        if (!objectRigidbody) return;

        objectRigidbody.linearVelocity = Vector3.zero;
        objectRigidbody.angularVelocity = Vector3.zero;
        objectRigidbody.transform.position = initialPosition;
        objectRigidbody.transform.rotation = initialRotation;

        // Add this line:
        if (distanceTimeGraphController != null)
        {
            distanceTimeGraphController.StopAndClearGraph();
        }

        Debug.Log("Object Reset");
    }

    // Optional: Update text fields if sliders are manipulated directly in editor during play
    void OnValidate()
    {
        if (gravityValueText && gravitySlider) gravityValueText.text = gravitySlider.value.ToString("F2");
        if (massValueText && massSlider) massValueText.text = massSlider.value.ToString("F2") + " kg";
        if (forceValueText && forceSlider) forceValueText.text = forceSlider.value.ToString("F0") + " N";
    }
}