using UnityEngine;
using System.Collections.Generic;
using TMPro; // Required for TextMeshPro

[RequireComponent(typeof(LineRenderer))]
public class LineGraphPlotter : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public RectTransform graphAreaRect; // Assign the "LineGraphArea" RectTransform
    public GameObject graphLabelPrefab; // Assign your "GraphLabelPrefab"

    [Header("Scale Settings")]
    public int numberOfXticks = 5;
    public int numberOfYticks = 5;
    public float xTickLabelVerticalOffset = -20f;  // Offset for X-axis tick labels
    public float yTickLabelHorizontalOffset = -35f; // Offset for Y-axis tick labels

    [Header("Origin Label Specifics")]
    public string originLabelText = "0";
    public Vector2 originLabelOffset = new Vector2(5f, -20f); // X, Y offset from bottom-left corner
    public Vector2 originLabelPivot = new Vector2(0f, 1f); // Pivot for the origin label (e.g., Top-Left)
    public TextAlignmentOptions originLabelAlignment = TextAlignmentOptions.TopLeft;


    [Header("Axis Titles")]
    public string xAxisTitle = "Time (s)";
    public string yAxisTitle = "Distance (m)";
    public float xAxisTitleVerticalOffset = -45f; // Offset for the X-axis title
    public float yAxisTitleHorizontalOffset = -60f; // Offset for the Y-axis title
    public int axisTitleFontSize = 14;


    private float graphTimeScaleMax = 10f;
    private float graphDistanceScaleMax = 100f;

    private List<Vector3> points = new List<Vector3>();
    private List<GameObject> scaleElements = new List<GameObject>(); // Holds all created labels and titles

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (graphAreaRect == null)
        {
            Debug.LogError("GraphAreaRect not assigned to LineGraphPlotter! Graph scaling/labels will be incorrect.", this);
        }
        if (graphLabelPrefab == null)
        {
            Debug.LogError("GraphLabelPrefab not assigned to LineGraphPlotter! Graph labels cannot be created.", this);
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = false; // Ensure line is drawn in local space of this object
        }
    }

    public void InitializeGraph(float theoreticalMaxTime, float theoreticalMaxDistance)
    {
        ClearGraphAndScales();

        graphTimeScaleMax = Mathf.Max(theoreticalMaxTime, 0.1f); // Avoid division by zero
        graphDistanceScaleMax = Mathf.Max(theoreticalMaxDistance, 0.1f); // Avoid division by zero

        DrawScalesAndTitles();
    }

    public void AddDataPoint(float currentTime, float currentHorizontalDistance)
    {
        if (graphAreaRect == null || lineRenderer == null) return;

        float graphWidth = graphAreaRect.rect.width;
        float graphHeight = graphAreaRect.rect.height;

        float xPos = (currentTime / graphTimeScaleMax) * graphWidth;
        float yPos = (currentHorizontalDistance / graphDistanceScaleMax) * graphHeight;

        // Clamp values to ensure they stay within the graph bounds
        xPos = Mathf.Clamp(xPos, 0, graphWidth);
        yPos = Mathf.Clamp(yPos, 0, graphHeight);

        Vector3 newPoint = new Vector3(xPos, yPos, 0); // Z is 0 in local space for LineRenderer
        points.Add(newPoint);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void ClearGraphAndScales()
    {
        points.Clear();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }

        foreach (GameObject elem in scaleElements)
        {
            if (elem != null) Destroy(elem);
        }
        scaleElements.Clear();
    }

    private TMP_Text CreateLabel(string text, Vector2 anchoredPosition, Transform parent,
                                 TextAlignmentOptions textAlignment, int fontSize = -1, Vector2? pivotOverride = null)
    {
        if (graphLabelPrefab == null)
        {
            Debug.LogError("Cannot create label, GraphLabelPrefab is not assigned.", this);
            return null;
        }

        GameObject labelGO = Instantiate(graphLabelPrefab, parent);
        scaleElements.Add(labelGO);

        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        TMP_Text labelText = labelGO.GetComponent<TMP_Text>();

        if (labelText == null || labelRect == null)
        {
            Debug.LogError("GraphLabelPrefab is missing TMP_Text or RectTransform.", labelGO);
            Destroy(labelGO); // Clean up if misconfigured
            scaleElements.Remove(labelGO); // Remove from list if destroyed
            return null;
        }

        labelText.text = text;
        labelText.alignment = textAlignment;

        if (fontSize > 0)
        {
            labelText.fontSize = fontSize;
        }

        if (pivotOverride.HasValue)
        {
            labelRect.pivot = pivotOverride.Value;
        }
        // Else, it uses the pivot from the GraphLabelPrefab

        labelRect.anchoredPosition = anchoredPosition;
        return labelText;
    }

    private void DrawScalesAndTitles()
    {
        if (graphAreaRect == null)
        {
            Debug.LogError("Cannot draw scales, GraphAreaRect is not assigned.", this);
            return;
        }

        float graphWidth = graphAreaRect.rect.width;
        float graphHeight = graphAreaRect.rect.height;

        // --- Draw Origin Label (0,0) ---
        // Ensure this is drawn only if ticks are expected, to avoid a lone "0" if no ticks are set
        if (numberOfXticks > 0 || numberOfYticks > 0)
        {
            CreateLabel(originLabelText,
                        originLabelOffset,
                        graphAreaRect,
                        originLabelAlignment,
                        -1, // Default font size from prefab
                        originLabelPivot);
        }

        // --- Draw X-axis Ticks and Labels (Time) ---
        // Assumes the GraphLabelPrefab has a pivot like (0.5, 1) [TopCenter] for these X-tick labels
        // if no pivotOverride is passed to CreateLabel for these.
        if (numberOfXticks > 0)
        {
            for (int i = 1; i <= numberOfXticks; i++) // Start from 1 to skip the origin
            {
                float normalizedX = (float)i / numberOfXticks;
                float xPos = normalizedX * graphWidth;
                float timeValue = normalizedX * graphTimeScaleMax;

                CreateLabel(timeValue.ToString("F1") + "s",
                            new Vector2(xPos, xTickLabelVerticalOffset),
                            graphAreaRect,
                            TextAlignmentOptions.Center); // Rely on prefab pivot or set one
            }
        }

        // --- Draw Y-axis Ticks and Labels (Distance) ---
        // Assumes the GraphLabelPrefab has a pivot like (1, 0.5) [MiddleRight] for these Y-tick labels
        // if no pivotOverride is passed to CreateLabel for these.
        if (numberOfYticks > 0)
        {
            for (int i = 1; i <= numberOfYticks; i++) // Start from 1 to skip the origin
            {
                float normalizedY = (float)i / numberOfYticks;
                float yPos = normalizedY * graphHeight;
                float distanceValue = normalizedY * graphDistanceScaleMax;

                CreateLabel(distanceValue.ToString("F1") + "m",
                            new Vector2(yTickLabelHorizontalOffset, yPos),
                            graphAreaRect,
                            TextAlignmentOptions.Right); // Rely on prefab pivot or set one
            }
        }

        // --- Draw Axis Titles ---
        // X-Axis Title (typically centered below the X-axis tick labels)
        if (!string.IsNullOrEmpty(xAxisTitle))
        {
            CreateLabel(xAxisTitle,
                        new Vector2(graphWidth / 2, xAxisTitleVerticalOffset),
                        graphAreaRect,
                        TextAlignmentOptions.Center,
                        axisTitleFontSize,
                        new Vector2(0.5f, 1f)); // Pivot: TopCenter for positioning below
        }

        // Y-Axis Title (typically centered to the left of Y-axis tick labels)
        if (!string.IsNullOrEmpty(yAxisTitle))
        {
            CreateLabel(yAxisTitle,
                        new Vector2(yAxisTitleHorizontalOffset, graphHeight / 2),
                        graphAreaRect,
                        TextAlignmentOptions.Center,
                        axisTitleFontSize,
                        new Vector2(1f, 0.5f)); // Pivot: MiddleRight for positioning to the left
        }
    }
}