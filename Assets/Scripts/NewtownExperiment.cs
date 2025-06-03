using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; // For List<Vector3>

public class NewtonFirstLawExperiment : MonoBehaviour
{
    
    [Header("UI Sliders")]
    public Slider gravityScaleSlider;
    public Slider forceScaleSlider;
    public Slider massScaleSlider;

    [Header("UI Slider Value Texts")]
    public TextMeshProUGUI gravityValueText;
    public TextMeshProUGUI forceValueText;
    public TextMeshProUGUI massValueText;

    [Header("UI Buttons")]
    public Button setParametersButton;
    public Button randomizeParametersButton;
    public Button applyAndCalculateButton;

    [Header("Object Reference")]
    public GameObject ballObject;
    private Rigidbody ballRigidbody;
    public LineRenderer ballTrailRenderer; 

    [Header("Stats Display")]
    public TextMeshProUGUI statsDisplayText;

    [Header("Simulation Settings")]
    public int observationDurationSeconds = 5;
    public float trailUpdateInterval = 0.1f; 

    private Vector3 actualInitialBallPosition;
    private readonly Vector3 defaultAppliedForceDirection = Vector3.forward;

    private float currentSetMass;
    private float currentSetGravityValue;
    private float currentSetForceMagnitude;

    private bool parametersHaveBeenSet = false;
    private Vector3 ballInitialVelocityForCalculation;

    private Coroutine activeForceAndTrailCoroutine = null;
    private List<Vector3> trailPoints = new List<Vector3>();

    
    private float currentVisualElapsedTime = 0f;
    private float currentVisualDistanceTraveled = 0f;
    private Vector3 visualSimulationStartPosition; 
    private bool isVisualSimulationRunning = false;
    

    void Start()
    {
      
        if (ballObject == null)
        {
            Debug.LogError("Ball Object is not assigned!");
            this.enabled = false;
            return;
        }

        ballRigidbody = ballObject.GetComponent<Rigidbody>();
        if (ballRigidbody == null)
        {
            Debug.LogError("Ball Object does not have a Rigidbody component!");
            this.enabled = false;
            return;
        }


        if (ballTrailRenderer == null)
        {
            Debug.LogWarning("Ball Object does not have a LineRenderer component! Trail will not be drawn. Please add one.");
        }
        else
        {
            ballTrailRenderer.positionCount = 0;
            ballTrailRenderer.useWorldSpace = true;
        }


        actualInitialBallPosition = ballObject.transform.position;

        if (observationDurationSeconds < 1)
        {
            Debug.LogWarning("Observation Duration Seconds was less than 1. Setting to 1 second.");
            observationDurationSeconds = 1;
        }
        if (trailUpdateInterval <= 0)
        {
            Debug.LogWarning("Trail Update Interval must be positive. Setting to 0.1s.");
            trailUpdateInterval = 0.1f;
        }


        ValidateSlider(gravityScaleSlider, "Gravity Scale Slider");
        ValidateSlider(forceScaleSlider, "Force Scale Slider");
        ValidateSlider(massScaleSlider, "Mass Scale Slider");
        ValidateText(gravityValueText, "Gravity Value Text");
        ValidateText(forceValueText, "Force Value Text");
        ValidateText(massValueText, "Mass Value Text");
        ValidateText(statsDisplayText, "Stats Display Text");
        ValidateButton(setParametersButton, "Set Parameters Button");
        ValidateButton(randomizeParametersButton, "Randomize Parameters Button");
        ValidateButton(applyAndCalculateButton, "Apply & Calculate Button");

        SetupSliderListeners();
        SetupButtonListeners();
        InitializeSliderTexts();
        ResetBallToInitialStateAndStopForces();

        statsDisplayText.text = $"Welcome! Set Observation Time (currently {observationDurationSeconds}s) in Inspector.\nAdjust sliders or randomize, then Set Parameters, then Apply Force.";
    }


    void ValidateSlider(Slider slider, string sliderName)
    {
        if (slider == null) Debug.LogError(sliderName + " is not assigned!", this);
    }
    void ValidateText(TextMeshProUGUI textElement, string textName)
    {
        if (textElement == null) Debug.LogError(textName + " is not assigned!", this);
    }
    void ValidateButton(Button button, string buttonName)
    {
        if (button == null) Debug.LogError(buttonName + " is not assigned!", this);
    }

    void SetupSliderListeners()
    {
        gravityScaleSlider.onValueChanged.AddListener(delegate { UpdateGravityText(); });
        forceScaleSlider.onValueChanged.AddListener(delegate { UpdateForceText(); });
        massScaleSlider.onValueChanged.AddListener(delegate { UpdateMassText(); });
    }

    void SetupButtonListeners()
    {
        setParametersButton.onClick.AddListener(SetObjectParameters);
        randomizeParametersButton.onClick.AddListener(RandomizeAllParameters);
        applyAndCalculateButton.onClick.AddListener(HandleApplyForceAndCalculations);
    }

    void InitializeSliderTexts()
    {
        UpdateGravityText(); UpdateForceText(); UpdateMassText();
    }

    void UpdateGravityText()
    {
        if (gravityValueText != null) gravityValueText.text = gravityScaleSlider.value.ToString("F2") + " m/s²";
    }
    void UpdateForceText()
    {
        if (forceValueText != null) forceValueText.text = forceScaleSlider.value.ToString("F2") + " N";
    }
    void UpdateMassText()
    {
        if (massValueText != null) massValueText.text = massScaleSlider.value.ToString("F2") + " kg";
    }


    void ResetBallToInitialStateAndStopForces()
    {
        isVisualSimulationRunning = false; 
        currentVisualElapsedTime = 0f;
        currentVisualDistanceTraveled = 0f;

        if (activeForceAndTrailCoroutine != null)
        {
            StopCoroutine(activeForceAndTrailCoroutine);
            activeForceAndTrailCoroutine = null;
        }

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.useGravity = false;
            ballObject.transform.position = actualInitialBallPosition;
        }
        ballInitialVelocityForCalculation = Vector3.zero;

        if (ballTrailRenderer != null)
        {
            ballTrailRenderer.positionCount = 0;
        }
        trailPoints.Clear();
    }

    public void RandomizeAllParameters()
    {
        gravityScaleSlider.value = Random.Range(gravityScaleSlider.minValue, gravityScaleSlider.maxValue);
        forceScaleSlider.value = Random.Range(forceScaleSlider.minValue, forceScaleSlider.maxValue);
        massScaleSlider.value = Random.Range(massScaleSlider.minValue, massScaleSlider.maxValue);
        statsDisplayText.text = "Parameters randomized. Review and click 'Set Parameters'.";
        parametersHaveBeenSet = false;
    }

    public void SetObjectParameters()
    {
     
        currentSetMass = massScaleSlider.value;
        currentSetGravityValue = gravityScaleSlider.value;
        currentSetForceMagnitude = forceScaleSlider.value;

        if (ballRigidbody != null)
        {
            ballRigidbody.mass = currentSetMass;
        }

        ResetBallToInitialStateAndStopForces();

        string parametersInfo = $"Mass (m): {currentSetMass:F2} kg\n" +
                                $"Gravity Value (g): {currentSetGravityValue:F2} m/s²\n" +
                                $"Applied Force (F_app): {currentSetForceMagnitude:F2} N\n" +
                                $"Observation Time (t): {observationDurationSeconds} s\n\n";
        statsDisplayText.text = parametersInfo; // Only show inputs here
        Debug.Log($"Parameters Set: Mass={currentSetMass:F2}kg, GravityVal={currentSetGravityValue:F2}m/s^2, ForceMag={currentSetForceMagnitude:F2}N, ObsTime={observationDurationSeconds}s");
        parametersHaveBeenSet = true;
    }

    public void HandleApplyForceAndCalculations()
    {
        if (!parametersHaveBeenSet)
        {
            statsDisplayText.text = "Please 'Set Parameters' before applying force."; // Clearer message
            Debug.LogWarning("Attempted to apply force before parameters were set.");
            return;
        }

        ResetBallToInitialStateAndStopForces();
        PerformTheoreticalCalculations(); // This updates statsDisplayText with theoretical results
        activeForceAndTrailCoroutine = StartCoroutine(ApplyForcesAndDrawTrailCoroutine());
    }

    IEnumerator ApplyForcesAndDrawTrailCoroutine()
    {
        isVisualSimulationRunning = true; // Mark simulation as running
        currentVisualElapsedTime = 0f;    // Reset live timer
        currentVisualDistanceTraveled = 0f; // Reset live distance
        visualSimulationStartPosition = ballObject.transform.position; // Capture start position for distance calculation

        float lastTrailUpdateTime = 0f;
        Vector3 visualForceToApply = defaultAppliedForceDirection.normalized * currentSetForceMagnitude;
        Vector3 visualGravityForce = Vector3.down * currentSetGravityValue * currentSetMass;
        float observationTimeFloat = (float)observationDurationSeconds;

        if (ballTrailRenderer != null)
        {
            trailPoints.Add(visualSimulationStartPosition); // Start trail from actual start
            ballTrailRenderer.positionCount = trailPoints.Count;
            ballTrailRenderer.SetPositions(trailPoints.ToArray());
        }

        while (currentVisualElapsedTime < observationTimeFloat)
        {
            ballRigidbody.AddForce(visualForceToApply, ForceMode.Force);
            if (currentSetGravityValue > 0.001f)
            {
                ballRigidbody.AddForce(visualGravityForce, ForceMode.Force);
            }

            if (ballTrailRenderer != null && (currentVisualElapsedTime - lastTrailUpdateTime) >= trailUpdateInterval)
            {
                trailPoints.Add(ballObject.transform.position);
                ballTrailRenderer.positionCount = trailPoints.Count;
                ballTrailRenderer.SetPositions(trailPoints.ToArray());
                lastTrailUpdateTime = currentVisualElapsedTime;
            }

            currentVisualElapsedTime += Time.deltaTime;
            currentVisualDistanceTraveled = Vector3.Distance(visualSimulationStartPosition, ballObject.transform.position);
            yield return null;
        }

        isVisualSimulationRunning = false; // Mark simulation as stopped

        if (ballRigidbody != null)
        {
            ballRigidbody.linearVelocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

        if (ballTrailRenderer != null)
        {
            trailPoints.Add(ballObject.transform.position); // Final point
            ballTrailRenderer.positionCount = trailPoints.Count;
            ballTrailRenderer.SetPositions(trailPoints.ToArray());
        }

        Debug.Log($"Visual force application and trail drawing finished after {currentVisualElapsedTime:F2} seconds. Ball stopped. Visual Distance: {currentVisualDistanceTraveled:F2}m");
        activeForceAndTrailCoroutine = null;
    }

    void PerformTheoreticalCalculations()
    {
        // ... (PerformTheoreticalCalculations logic remains the same, it populates statsDisplayText) ...
        ballInitialVelocityForCalculation = Vector3.zero;
        float initialSpeed = ballInitialVelocityForCalculation.magnitude;

        Vector3 appliedForceVector = defaultAppliedForceDirection.normalized * currentSetForceMagnitude;
        Vector3 gravitationalForceVector = Vector3.down * currentSetGravityValue * currentSetMass;

        Vector3 totalNetForce = appliedForceVector;
        if (currentSetGravityValue > 0.001f)
        {
            totalNetForce += gravitationalForceVector;
        }

        float accelerationMagnitude = 0f;
        if (currentSetMass > 0.0001f)
        {
            accelerationMagnitude = totalNetForce.magnitude / currentSetMass;
        }
        else
        {
            Debug.LogWarning("Mass is zero or too small, acceleration is theoretically infinite.");
            accelerationMagnitude = float.PositiveInfinity;
        }

        float timeForCalc = (float)observationDurationSeconds;
        float finalSpeed = accelerationMagnitude * timeForCalc;
        float distanceTraveled = 0.5f * accelerationMagnitude * Mathf.Pow(timeForCalc, 2);

        string calculationResults = "<b>Theoretical Calculation Outputs:</b>\n" + // Added title for clarity
                                   $"<color=yellow>INPUTS (used for calculation):</color>\n" +
                                   $"  Mass: {currentSetMass:F2} kg\n" +
                                   $"  Gravity Value (g): {currentSetGravityValue:F2} m/s²\n" +
                                   $"  Applied Force: {currentSetForceMagnitude:F2} N\n\n" +
                                   $"<color=yellow>OUTPUTS (Calculated Scalars for t = {timeForCalc:F0}s):</color>\n" +
                                   $"  <b>Time: {timeForCalc:F2} s</b>\n" +
                                   $"  <b>Net Acceleration: {accelerationMagnitude:F2} m/s²</b>\n" +
                                   $"  <b>Final Velocity (Speed): {finalSpeed:F2} m/s</b>\n" +
                                   $"  <b>Distance Traveled: {distanceTraveled:F2} m</b>\n\n";

        if (totalNetForce.magnitude < 0.001f && currentSetForceMagnitude < 0.001f && currentSetGravityValue < 0.001f)
        {
            calculationResults += "<i>Observation: Approx. Zero Net Force. Object at rest tends to stay at rest (Newton's 1st Law).</i>";
        }
        else if (totalNetForce.magnitude < 0.001f && (currentSetForceMagnitude > 0.001f || currentSetGravityValue > 0.001f))
        {
            calculationResults += "<i>Observation: Forces are balanced, resulting in approx. Zero Net Force. Object at rest tends to stay at rest (Newton's 1st Law).</i>";
        }
        else
        {
            calculationResults += "<i>Observation: Non-Zero Net Force causing acceleration (Newton's 2nd Law).</i>";
        }

        statsDisplayText.text = calculationResults; // Update TMP text with theoretical results
        Debug.Log($"THEORETICAL OUTPUTS (t={timeForCalc:F0}s): Time={timeForCalc:F2}s, Accel={accelerationMagnitude:F2}m/s^2, FinalSpeed={finalSpeed:F2}m/s, Distance={distanceTraveled:F2}m");

    }

    // --- NEW GETTER FUNCTIONS ---
    /// <summary>
    /// Gets the elapsed time of the current visual simulation in seconds.
    /// Returns 0 if the simulation is not running.
    /// </summary>
    public float GetCurrentVisualElapsedTime()
    {
        return isVisualSimulationRunning ? currentVisualElapsedTime : 0f;
    }

    /// <summary>
    /// Gets the distance traveled by the ball from its starting point during the current visual simulation.
    /// Returns 0 if the simulation is not running.
    /// </summary>
    public float GetCurrentVisualDistanceTraveled()
    {
        return isVisualSimulationRunning ? currentVisualDistanceTraveled : 0f;
    }
    public bool GetIsVisualSimulationCurrentlyRunning()
    {
        return isVisualSimulationRunning;
    }
    // --------------------------
}