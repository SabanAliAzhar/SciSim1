using UnityEngine;
using UnityEngine.UI; // Required for Image and Mask
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GraphPlotter : MonoBehaviour
{
    [Header("Experiment Reference")]
    public NewtonFirstLawExperiment newtonExperiment;

    [Header("Graph Plotting Area")]
    public RectTransform plottingAreaRectTransform; 

    [Header("Graph Dot Prefab")]
    public GameObject dotPrefab;

    [Header("UI Labels (Assign in Inspector)")]
    public TextMeshProUGUI xAxisLabel;
    public TextMeshProUGUI yAxisLabel;
    public TextMeshProUGUI xMinLabel;
    public TextMeshProUGUI xMaxLabel;
    public TextMeshProUGUI yMinLabel;
    public TextMeshProUGUI yMaxLabel;

    [Header("Graph Settings")]
    public int maxDataPointsToStore = 500;
    public int maxDotsToDisplay = 100;
    public float graphPadding = 15f; 
    public float minTimeBetweenDataPoints = 0.05f;

    private List<Vector2> collectedDataPoints = new List<Vector2>();
    private List<GameObject> activeVisualDots = new List<GameObject>();
    private bool wasNewtonSimulationRunningLastFrame = false;
    private float lastDataPointTime = -1f;

    private float currentGraphMinX, currentGraphMaxX, currentGraphMinY, currentGraphMaxY;

    void Awake()
    {
        if (newtonExperiment == null)
        {
            Debug.LogError("GraphPlotter: NewtonFirstLawExperiment reference not set!", this);
            enabled = false; return;
        }
        if (plottingAreaRectTransform == null)
        {
            Debug.LogError("GraphPlotter: Plotting Area RectTransform not set! This is where dots will be drawn.", this);
            enabled = false; return;
        }
        if (plottingAreaRectTransform.pivot.x != 0 || plottingAreaRectTransform.pivot.y != 0)
        {
            Debug.LogWarning("GraphPlotter: For correct bottom-left origin, the 'Plotting Area RectTransform' should have its Pivot set to (X:0, Y:0) in the Inspector.", this);
        }
        if (plottingAreaRectTransform.GetComponent<Mask>() == null)
        {
            Debug.LogWarning("GraphPlotter: Plotting Area RectTransform does not have a Mask component. Dots may draw outside its bounds. Please add a UI Mask component.", this);
        }
        if (dotPrefab == null)
        {
            Debug.LogWarning("GraphPlotter: Dot Prefab not assigned! Graph points will not be visible.", this);
        }

        if (xAxisLabel == null) Debug.LogWarning("GraphPlotter: X Axis Label not assigned.", this);
        if (yAxisLabel == null) Debug.LogWarning("GraphPlotter: Y Axis Label (for Distance) not assigned.", this);

        InitializeLabels();
    }

    void InitializeLabels()
    {
        if (xAxisLabel != null) xAxisLabel.text = "Time (s)";
        if (yAxisLabel != null) yAxisLabel.text = "Distance (m)";
        UpdateAxisValueDisplay(0, 0, 0, 0, true);
    }

    void Update()
    {
        if (newtonExperiment == null || !newtonExperiment.isActiveAndEnabled) return;

        bool isNewtonSimCurrentlyRunning = newtonExperiment.GetIsVisualSimulationCurrentlyRunning();
        float liveTime = newtonExperiment.GetCurrentVisualElapsedTime();
        float liveDistance = newtonExperiment.GetCurrentVisualDistanceTraveled();

        if (isNewtonSimCurrentlyRunning)
        {
            if (!wasNewtonSimulationRunningLastFrame)
            {
                collectedDataPoints.Clear();
                ClearAllVisuals();
                Debug.Log("[GraphPlotter] Newton experiment visual simulation detected as started. Clearing graph.");
                lastDataPointTime = -1f;
            }

            if (liveTime > lastDataPointTime + minTimeBetweenDataPoints)
            {
                collectedDataPoints.Add(new Vector2(liveTime, liveDistance));
                lastDataPointTime = liveTime;

                while (collectedDataPoints.Count > maxDataPointsToStore && maxDataPointsToStore > 0)
                {
                    collectedDataPoints.RemoveAt(0);
                }
                RedrawPlot();
            }
        }
        else
        {
            if (wasNewtonSimulationRunningLastFrame)
            {
                Debug.Log("[GraphPlotter] Newton experiment visual simulation detected as stopped. Plotting final point.");
                if (collectedDataPoints.Count == 0 ||
                    !Mathf.Approximately(collectedDataPoints.Last().x, liveTime) ||
                    !Mathf.Approximately(collectedDataPoints.Last().y, liveDistance))
                {
                    if (liveTime > 0.001f || collectedDataPoints.Any())
                    {
                        collectedDataPoints.Add(new Vector2(liveTime, liveDistance));
                        Debug.Log($"[GraphPlotter] Added final simulation state point: Time={liveTime:F3}s, Distance={liveDistance:F3}m");
                        while (collectedDataPoints.Count > maxDataPointsToStore && maxDataPointsToStore > 0)
                        {
                            collectedDataPoints.RemoveAt(0);
                        }
                    }
                }
                RedrawPlot();
                lastDataPointTime = -1f;
            }
        }
        wasNewtonSimulationRunningLastFrame = isNewtonSimCurrentlyRunning;
    }

    void RedrawPlot()
    {
        ClearInstantiatedDots();

        if (collectedDataPoints == null || collectedDataPoints.Count == 0)
        {
            UpdateAxisValueDisplay(0, 0, 0, 0, true);
            return;
        }

        List<Vector2> pointsToDisplayAsDots;
        if (collectedDataPoints.Count > maxDotsToDisplay && maxDotsToDisplay > 0)
        {
            pointsToDisplayAsDots = collectedDataPoints.Skip(collectedDataPoints.Count - maxDotsToDisplay).ToList();
        }
        else
        {
            pointsToDisplayAsDots = new List<Vector2>(collectedDataPoints);
        }

        if (pointsToDisplayAsDots.Count < 1)
        {
            UpdateAxisValueDisplay(0, 0, 0, 0, true);
            return;
        }

        float actualPlotWidth = plottingAreaRectTransform.rect.width - (2 * graphPadding);
        float actualPlotHeight = plottingAreaRectTransform.rect.height - (2 * graphPadding);

        if (actualPlotWidth <= 0 || actualPlotHeight <= 0)
        {
            Debug.LogWarning("[GraphPlotter] Plot area width or height is zero or negative based on plottingAreaRectTransform. Cannot draw graph.");
            UpdateAxisValueDisplay(0, 0, 0, 0, true);
            return;
        }

        currentGraphMinX = collectedDataPoints.Min(p => p.x);
        currentGraphMaxX = collectedDataPoints.Max(p => p.x);
        currentGraphMinY = 0f; // Distance always starts from 0 for the Y-axis minimum
        currentGraphMaxY = collectedDataPoints.Max(p => p.y);

        if (currentGraphMaxX <= currentGraphMinX) currentGraphMaxX = currentGraphMinX + 1.0f;
        if (currentGraphMaxY <= currentGraphMinY) currentGraphMaxY = currentGraphMinY + 1.0f; // If minY is 0, maxY becomes at least 1

        UpdateAxisValueDisplay(currentGraphMinX, currentGraphMaxX, currentGraphMinY, currentGraphMaxY);

        if (dotPrefab == null) return;

        foreach (Vector2 dataPoint in pointsToDisplayAsDots)
        {
            float normX = (currentGraphMaxX > currentGraphMinX) ? (dataPoint.x - currentGraphMinX) / (currentGraphMaxX - currentGraphMinX) : 0f;
            float normY = (currentGraphMaxY > currentGraphMinY) ? (dataPoint.y - currentGraphMinY) / (currentGraphMaxY - currentGraphMinY) : 0f;

            // Calculate position within the plottingAreaRectTransform's local space.
            // This calculation ASSUMES that plottingAreaRectTransform's PIVOT IS (0,0) (bottom-left).
            float dotLocalX = normX * actualPlotWidth + graphPadding;
            float dotLocalY = normY * actualPlotHeight + graphPadding;

            GameObject dotInstance = Instantiate(dotPrefab, plottingAreaRectTransform);
            RectTransform dotRect = dotInstance.GetComponent<RectTransform>();
            if (dotRect != null)
            {
                dotRect.anchoredPosition = new Vector2(dotLocalX, dotLocalY);
            }
            activeVisualDots.Add(dotInstance);
        }
    }

    void UpdateAxisValueDisplay(float minX, float maxX, float minY, float maxY, bool isEmpty = false)
    {
        if (isEmpty)
        {
            if (xMinLabel != null) xMinLabel.text = "0.0";
            if (xMaxLabel != null) xMaxLabel.text = "";
            if (yMinLabel != null) yMinLabel.text = "0.0";
            if (yMaxLabel != null) yMaxLabel.text = "";
            return;
        }
        if (xMinLabel != null) xMinLabel.text = minX.ToString("F1");
        if (xMaxLabel != null) xMaxLabel.text = maxX.ToString("F1");
        if (yMinLabel != null) yMinLabel.text = minY.ToString("F1");
        if (yMaxLabel != null) yMaxLabel.text = maxY.ToString("F1");
    }

    void ClearInstantiatedDots()
    {
        foreach (GameObject dot in activeVisualDots)
        {
            Destroy(dot);
        }
        activeVisualDots.Clear();
    }

    void ClearAllVisuals()
    {
        ClearInstantiatedDots();
        UpdateAxisValueDisplay(0, 0, 0, 0, true);
    }
}