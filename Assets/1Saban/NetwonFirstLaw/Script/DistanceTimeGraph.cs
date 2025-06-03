using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class DistanceTimeGraph : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("References")]
    public Rigidbody objectToTrack;
    public RawImage graphRawImage;

    [Header("UI Text Elements for Labels")]
    public TMP_Text graphTitleText;
    public TMP_Text yAxisLabelText;
    public TMP_Text xAxisLabelText;
    public TMP_Text hoverDataText;
    public TMP_Text maxTimeDisplayText;
    public TMP_Text maxDistanceDisplayText;
    public TMP_Text actualDistanceCoveredText; // << NEW UI ELEMENT REFERENCE

    [Header("Graph Parameters")]
    public string graphTitle = "Distance vs. Time";
    public string yAxisLabel = "Distance (m)";
    public string xAxisLabel = "Time (s)";
    public float defaultMaxTimeSeconds = 10f;
    public float defaultMaxDistanceMeters = 5f;
    public int lineThickness = 2;

    public Color graphBackgroundColor = Color.white;
    public Color lineColor = Color.blue;
    public Color axisColor = Color.black;

    private Texture2D graphTexture;
    private List<Vector2> dataPoints;

    private bool isRecording = false;
    private float startTime;
    private Vector3 recordingStartPosition;
    private int textureWidth;
    private int textureHeight;

    private float currentMaxTime;
    private float currentMaxDistance;
    private float lastRecordedActualDistance = 0f; // << NEW: To store the last actual distance

    private bool isPointerOverGraph = false;
    private RectTransform graphRectTransform;


    void Start()
    {
        if (!objectToTrack || !graphRawImage)
        {
            Debug.LogError("DistanceTimeGraph: Object to track or RawImage not assigned!");
            enabled = false;
            return;
        }

        graphRectTransform = graphRawImage.GetComponent<RectTransform>();
        textureWidth = (int)graphRectTransform.rect.width;
        textureHeight = (int)graphRectTransform.rect.height;

        if (textureWidth <= 0 || textureHeight <= 0)
        {
            Debug.LogError("Graph RawImage has zero or negative width/height. Ensure its RectTransform is properly set up.");
            enabled = false;
            return;
        }

        graphTexture = new Texture2D(textureWidth, textureHeight);
        graphRawImage.texture = graphTexture;
        dataPoints = new List<Vector2>();

        if (graphTitleText) graphTitleText.text = graphTitle;
        if (yAxisLabelText) yAxisLabelText.text = yAxisLabel;
        if (xAxisLabelText) xAxisLabelText.text = xAxisLabel;
        if (hoverDataText) hoverDataText.gameObject.SetActive(false);

        currentMaxTime = defaultMaxTimeSeconds;
        currentMaxDistance = defaultMaxDistanceMeters;
        UpdateScaleDisplayText();
        UpdateActualDistanceDisplay(0f); // Initialize actual distance display

        ClearGraphAndDrawBase();
    }

    void Update()
    {
        if (isRecording && objectToTrack)
        {
            float elapsedTime = Time.time - startTime;
            float currentActualDistance = Vector3.Distance(objectToTrack.transform.position, recordingStartPosition);
            lastRecordedActualDistance = currentActualDistance; // Store it
            UpdateActualDistanceDisplay(currentActualDistance); // << UPDATE UI

            // --- TEMPORARILY COMMENT OUT DYNAMIC SCALING FOR "STRAIGHT LINE" TEST ---
            // You can re-enable this later if desired, or make it an option
            
            bool scaleChanged = false;
            if (elapsedTime > currentMaxTime)
            {
                currentMaxTime *= 1.5f;
                scaleChanged = true;
            }
            if (currentActualDistance > currentMaxDistance) // Use currentActualDistance here
            {
                currentMaxDistance *= 1.5f;
                scaleChanged = true;
            }

            if (scaleChanged)
            {
                UpdateScaleDisplayText();
            }
            


            // Optional: Stop recording if time exceeds max displayable time (if not using dynamic scaling)
            if (!IsDynamicScalingEnabled() && (elapsedTime > currentMaxTime || currentActualDistance > currentMaxDistance))
            {
                 StopRecording(); // Or let it plot off-screen
            }


            dataPoints.Add(new Vector2(elapsedTime, currentActualDistance));
            RedrawGraphPlot();
        }
        else if (!isRecording && lastRecordedActualDistance > 0)
        {
            // Optionally keep displaying the last recorded actual distance when stopped
            // Or reset it in StopAndClearGraph
        }
    }

    // Helper to check if dynamic scaling is active (based on commented code)
    bool IsDynamicScalingEnabled()
    {
        // Return true if the dynamic scaling block in Update() is uncommented
        // For this example, let's assume it's false if that block is commented out
        return false; // Change to true if you uncomment the scaling block
    }


    void UpdateScaleDisplayText()
    {
        if (maxTimeDisplayText) maxTimeDisplayText.text = $"Max T: {currentMaxTime:F1}s";
        if (maxDistanceDisplayText) maxDistanceDisplayText.text = $"Max D: {currentMaxDistance:F1}m";
    }

    void UpdateActualDistanceDisplay(float distance) // << NEW METHOD
    {
        if (actualDistanceCoveredText)
        {
            actualDistanceCoveredText.text = $"Actual Dist: {distance:F2}m";
        }
    }

    public bool IsGraphRecording()
    {
        return isRecording;
    }

    public void StartRecording()
    {
        isRecording = true;
        dataPoints.Clear();
        startTime = Time.time;
        recordingStartPosition = objectToTrack.transform.position;
        lastRecordedActualDistance = 0f; // Reset

        currentMaxTime = defaultMaxTimeSeconds;
        currentMaxDistance = defaultMaxDistanceMeters;
        UpdateScaleDisplayText();
        UpdateActualDistanceDisplay(0f); // Reset display

        ClearGraphAndDrawBase();
        if (hoverDataText) hoverDataText.gameObject.SetActive(false);
    }

    public void StopRecording()
    {
        isRecording = false;
     
        UpdateActualDistanceDisplay(lastRecordedActualDistance); // Optionally update one last time
    }

    public void StopAndClearGraph()
    {
        isRecording = false;
        // Debug.Log($"GRAPH: Final Actual Recorded Distance: {lastRecordedActualDistance}"); // Log final value
        dataPoints.Clear();
        ClearGraphAndDrawBase();
        if (hoverDataText) hoverDataText.gameObject.SetActive(false);

        currentMaxTime = defaultMaxTimeSeconds;
        currentMaxDistance = defaultMaxDistanceMeters;
        UpdateScaleDisplayText();
        UpdateActualDistanceDisplay(0f); // Reset display on full clear
        lastRecordedActualDistance = 0f;
    }

    void ClearGraphAndDrawBase()
    {
        Color[] blankPixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < blankPixels.Length; i++)
        {
            blankPixels[i] = graphBackgroundColor;
        }
        graphTexture.SetPixels(0, 0, textureWidth, textureHeight, blankPixels);
        DrawAxes();
        graphTexture.Apply();
    }

    void DrawAxes()
    {
        for (int y = 0; y < textureHeight; y++)
        {
            graphTexture.SetPixel(0, y, axisColor);
            if (lineThickness > 1) graphTexture.SetPixel(1, y, axisColor);
        }
        for (int x = 0; x < textureWidth; x++)
        {
            graphTexture.SetPixel(x, 0, axisColor);
            if (lineThickness > 1) graphTexture.SetPixel(x, 1, axisColor);
        }
    }

    void RedrawGraphPlot()
    {
        ClearGraphAndDrawBase();

        if (dataPoints.Count < 2)
        {
            if (dataPoints.Count == 1)
            {
                Vector2 point = dataPoints[0];
                float xNorm = Mathf.Clamp01(point.x / currentMaxTime);
                // Use point.y (actual distance) for plotting against currentMaxDistance scale
                float yNorm = Mathf.Clamp01(point.y / currentMaxDistance);
                int xPixel = (int)(xNorm * (textureWidth - 1));
                int yPixel = (int)(yNorm * (textureHeight - 1));
                DrawThickPixel(xPixel, yPixel, lineColor, lineThickness);
            }
            graphTexture.Apply();
            return;
        }

        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            Vector2 p1 = dataPoints[i];
            Vector2 p2 = dataPoints[i + 1];

            float x1Norm = p1.x / currentMaxTime;
            float y1Norm = p1.y / currentMaxDistance; // Plot actual distance against current Y scale
            int x1Pixel = (int)(Mathf.Clamp01(x1Norm) * (textureWidth - 1));
            int y1Pixel = (int)(Mathf.Clamp01(y1Norm) * (textureHeight - 1));

            float x2Norm = p2.x / currentMaxTime;
            float y2Norm = p2.y / currentMaxDistance; // Plot actual distance against current Y scale
            int x2Pixel = (int)(Mathf.Clamp01(x2Norm) * (textureWidth - 1));
            int y2Pixel = (int)(Mathf.Clamp01(y2Norm) * (textureHeight - 1));

            if ((x1Norm >= 0 && x1Norm <= 1 && y1Norm >= 0 && y1Norm <= 1) ||
                 (x2Norm >= 0 && x2Norm <= 1 && y2Norm >= 0 && y2Norm <= 1))
            {
                DrawLine(x1Pixel, y1Pixel, x2Pixel, y2Pixel, lineColor, lineThickness);
            }
        }
        graphTexture.Apply();
    }

    void DrawThickPixel(int cx, int cy, Color color, int thickness)
    {
        int halfThickness = thickness / 2;
        for (int x = -halfThickness; x <= halfThickness; x++)
        {
            for (int y = -halfThickness; y <= halfThickness; y++)
            {
                int drawX = cx + x;
                int drawY = cy + y;
                if (drawX >= 0 && drawX < textureWidth && drawY >= 0 && drawY < textureHeight)
                {
                    graphTexture.SetPixel(drawX, drawY, color);
                }
            }
        }
    }

    void DrawLine(int x0, int y0, int x1, int y1, Color color, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawThickPixel(x0, y0, color, thickness);
            if ((x0 == x1) && (y0 == y1)) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == graphRawImage.gameObject)
        {
            isPointerOverGraph = true;
            if (hoverDataText) hoverDataText.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverGraph = false;
        if (hoverDataText) hoverDataText.gameObject.SetActive(false);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isPointerOverGraph || dataPoints.Count == 0 || !hoverDataText) return;
        if (eventData.pointerCurrentRaycast.gameObject != graphRawImage.gameObject) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            graphRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        float normalizedX = (localPoint.x / graphRectTransform.rect.width) + graphRectTransform.pivot.x;

        if (normalizedX < 0 || normalizedX > 1)
        {
            if (hoverDataText.IsActive()) hoverDataText.gameObject.SetActive(false);
            return;
        }
        if (!hoverDataText.IsActive()) hoverDataText.gameObject.SetActive(true);

        float hoverTime = normalizedX * currentMaxTime;
        Vector2 closestPoint = new Vector2(-1, -1);
        float minTimeDiff = float.MaxValue;

        foreach (var point in dataPoints)
        {
            float diff = Mathf.Abs(point.x - hoverTime);
            if (diff < minTimeDiff)
            {
                minTimeDiff = diff;
                closestPoint = point;
            }
        }

        if (closestPoint.x >= 0)
        {
            hoverDataText.text = $"Time: {closestPoint.x:F2}s\nDist: {closestPoint.y:F2}m";
        }
        else
        {
            if (hoverDataText.IsActive()) hoverDataText.gameObject.SetActive(false);
        }
    }
}