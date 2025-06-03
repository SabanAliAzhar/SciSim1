using UnityEngine;
using TMPro; // For TextMeshPro elements
using UnityEngine.UI; // For Button

public class ProjectileLauncher : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField velocityInputField;
    public TMP_InputField angleInputField;
    public Button launchButton;
    public Button resetButton;
    public TMP_Text timeOfFlightText;
    public TMP_Text rangeText;
    public TMP_Text maxHeightText;

    [Header("Graphing")]
    public LineGraphPlotter lineGraphPlotter; // Assign your GraphLineObject (which has LineGraphPlotter.cs)
    public float plotInterval = 0.1f;


    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform launchPoint;
    public float gravity = 9.81f;

    [Header("Default Values for Reset")]
    public string defaultVelocity = "10";
    public string defaultAngle = "45";

    private GameObject currentProjectile;
    private Rigidbody projectileRb;
    private bool isSimulating = false;
    private float launchTime;
    private float maxSimulatedHeight;
    private Vector3 initialLaunchPosition;
    private float nextPlotTime = 0f;


    void Start()
    {
        if (launchButton != null)
        {
            launchButton.onClick.AddListener(Launch);
        }
        else
        {
            Debug.LogError("Launch Button not assigned!", this);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetSimulation);
        }
        else
        {
            Debug.LogWarning("Reset Button not assigned (optional).", this);
        }

        if (lineGraphPlotter == null)
        {
            Debug.LogWarning("LineGraphPlotter not assigned to ProjectileLauncher. Graphing will be disabled.", this);
        }

        InitializeUIState();
    }

    void InitializeUIState()
    {
        ResetInputFields();
        ClearOutputMetrics();
        if (lineGraphPlotter != null)
        {
            // The ClearGraphAndScales method in LineGraphPlotter will be called
            // when InitializeGraph is called, or if ResetSimulation calls this.
            // For the very first start, we can ensure it's clear.
            lineGraphPlotter.ClearGraphAndScales();
        }
    }

    public void Launch()
    {
        if (projectilePrefab == null || launchPoint == null)
        {
            Debug.LogError("Projectile Prefab or Launch Point not assigned!", this);
            return;
        }

        if (!float.TryParse(velocityInputField.text, out float initialVelocity) ||
            !float.TryParse(angleInputField.text, out float launchAngle))
        {
            Debug.LogError("Invalid input for velocity or angle.", this);
            UpdateUIResults(0, 0, 0, true, "Invalid Input");
            if (lineGraphPlotter != null) lineGraphPlotter.ClearGraphAndScales();
            return;
        }

        if (currentProjectile != null)
        {
            Destroy(currentProjectile);
        }

        // --- 1. Theoretical Calculations ---
        float angleRad = launchAngle * Mathf.Deg2Rad;
        float v0x = initialVelocity * Mathf.Cos(angleRad);
        float v0y = initialVelocity * Mathf.Sin(angleRad);

        float theoreticalToF = (2 * v0y) / gravity;
        float theoreticalRange = Mathf.Max(0.01f, v0x * theoreticalToF); // Ensure not zero for graph
        float theoreticalMaxHeight = (v0y * v0y) / (2 * gravity);

        UpdateUIResults(theoreticalToF, theoreticalRange, theoreticalMaxHeight, true);

        // Initialize Graph Plotter
        if (lineGraphPlotter != null)
        {
            lineGraphPlotter.InitializeGraph(Mathf.Max(0.1f, theoreticalToF), theoreticalRange);
        }
        nextPlotTime = Time.time; // Start plotting immediately

        // --- 2. Instantiate and Launch Projectile ---
        currentProjectile = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation);
        projectileRb = currentProjectile.GetComponent<Rigidbody>();

        if (projectileRb == null)
        {
            Debug.LogError("Projectile prefab needs a Rigidbody!", this);
            Destroy(currentProjectile);
            return;
        }

        Vector3 launchDirection = launchPoint.forward * v0x + launchPoint.up * v0y;
        projectileRb.linearVelocity = launchDirection;

        // --- 3. Prepare for Simulated Metrics & Graphing ---
        initialLaunchPosition = launchPoint.position;
        launchTime = Time.time;
        maxSimulatedHeight = initialLaunchPosition.y;
        isSimulating = true;

        ProjectileCollisionDetector detector = currentProjectile.GetComponent<ProjectileCollisionDetector>();
        if (detector == null)
        {
            detector = currentProjectile.AddComponent<ProjectileCollisionDetector>();
        }
        detector.launcher = this; // Assuming ProjectileCollisionDetector script exists and has a public 'launcher' field
    }

    public void ResetSimulation()
    {
        if (currentProjectile != null)
        {
            Destroy(currentProjectile);
            currentProjectile = null;
            projectileRb = null;
        }
        isSimulating = false;

        InitializeUIState(); // This calls ClearGraphAndScales on lineGraphPlotter
        Debug.Log("Simulation Reset!", this);
    }

    private void ResetInputFields()
    {
        if (velocityInputField != null)
        {
            velocityInputField.text = defaultVelocity;
        }
        if (angleInputField != null)
        {
            angleInputField.text = defaultAngle;
        }
    }

    private void ClearOutputMetrics()
    {
        UpdateUIResults(0, 0, 0, true, "--- Awaiting Launch ---");
    }

    void Update()
    {
        if (isSimulating && currentProjectile != null)
        {
            // Max Height Calculation
            float currentYPos = currentProjectile.transform.position.y;
            if (currentYPos > maxSimulatedHeight)
            {
                maxSimulatedHeight = currentYPos;
            }

            // Graph Plotting Logic
            if (lineGraphPlotter != null && Time.time >= nextPlotTime)
            {
                float elapsedTime = Time.time - launchTime;
                Vector3 currentPos = currentProjectile.transform.position;
                Vector3 initialPosXZ = new Vector3(initialLaunchPosition.x, 0, initialLaunchPosition.z);
                Vector3 currentPosXZ = new Vector3(currentPos.x, 0, currentPos.z);
                float currentHorizontalDistance = Vector3.Distance(initialPosXZ, currentPosXZ);

                lineGraphPlotter.AddDataPoint(elapsedTime, currentHorizontalDistance);
                nextPlotTime = Time.time + plotInterval;
            }
        }
    }

    public void HandleProjectileLanded(Vector3 landingPosition)
    {
        if (!isSimulating) return;

        float simulatedToF = Time.time - launchTime;
        // Stop simulation updates BEFORE final plot point and UI update
        isSimulating = false;

        Vector3 launchPosXZ = new Vector3(initialLaunchPosition.x, 0, initialLaunchPosition.z);
        Vector3 landPosXZ = new Vector3(landingPosition.x, 0, landingPosition.z);
        float simulatedRange = Vector3.Distance(launchPosXZ, landPosXZ);
        float simulatedMaxHeightAchieved = maxSimulatedHeight - initialLaunchPosition.y;

        // Plot final point before updating UI text
        if (lineGraphPlotter != null)
        {
            lineGraphPlotter.AddDataPoint(simulatedToF, simulatedRange);
        }

        UpdateUIResults(simulatedToF, simulatedRange, simulatedMaxHeightAchieved, false);


        if (projectileRb != null)
        {
            projectileRb.isKinematic = true;
        }
    }

    private void UpdateUIResults(float tof, float range, float height, bool isTheoretical, string customMessage = "")
    {
        string prefix = isTheoretical ? "Theoretical " : "Simulated ";
        if (!string.IsNullOrEmpty(customMessage))
        {
            timeOfFlightText.text = customMessage;
            rangeText.text = "";
            maxHeightText.text = "";
            return;
        }

        timeOfFlightText.text = prefix + "Time of Flight: " + tof.ToString("F2") + " s";
        rangeText.text = prefix + "Range: " + range.ToString("F2") + " m";
        maxHeightText.text = prefix + "Max Height: " + height.ToString("F2") + " m";
    }
}