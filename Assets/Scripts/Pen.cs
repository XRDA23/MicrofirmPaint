using UnityEngine;

public class Pen : MonoBehaviour
{
    [Header("Pen Properties")] public Transform tip;
    public Material drawingMaterial; // Material assigned to the line being drawn
    public Material tipMaterial; // Material assigned to the pen's tip
    [Range(0.01f, 0.1f)] public float penWidth = 0.01f; // Width of the pen's line, with a slider in the inspector
    public Color[] penColors; // Array to hold different color options for the pen

    [Header("Hands & Grabbable")] public OVRGrabber rightHand;
    public OVRGrabber leftHand;
    public OVRGrabbable grabbable;

    private LineRenderer currentDrawing;
    private int index; // Index to keep track of the current position in the line's position array
    private int currentColorIndex; // Index to keep track of the current color selected

    // Event that gets invoked when drawing starts
    public delegate void StartDrawingHandler();

    public event StartDrawingHandler OnStartDrawing;

    // Event that gets invoked when drawing ends
    public delegate void EndDrawingHandler();

    public event EndDrawingHandler OnEndDrawing;

    // Event that gets invoked when color is switched
    public delegate void SwitchColorHandler(Color newColor);

    public event SwitchColorHandler OnSwitchColor;

    private void Start()
    {
        // Initialization and null checks
        grabbable = GetComponent<OVRGrabbable>();
        if (tip == null) Debug.LogError("Pen tip not assigned!");
        if (rightHand == null) Debug.LogError("Right hand not assigned!");
        if (leftHand == null) Debug.LogError("Left hand not assigned!");
        if (penColors.Length == 0) Debug.LogError("Pen colors array is empty!");

        currentColorIndex = 0;
        SetTipColor();
    }

    private void Update()
    {
        // Check if the pen is grabbed
        bool isGrabbed = grabbable.isGrabbed;

        // Check if the right hand is drawing
        bool isRightHandDrawing = isGrabbed && grabbable.grabbedBy == rightHand &&
                                  OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);

        // Check if the left hand is drawing
        bool isLeftHandDrawing = isGrabbed && grabbable.grabbedBy == leftHand &&
                                 OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);

        // If either the right hand or the left hand is drawing
        if (isRightHandDrawing || isLeftHandDrawing)
        {
            // If this is the start of a new drawing, invoke the OnStartDrawing event
            if (currentDrawing == null) OnStartDrawing?.Invoke();

            // Handle drawing
            Draw();
        }
        // If neither hand is drawing and there is an ongoing drawing
        else if (currentDrawing != null)
        {
            // Set currentDrawing to null, ending the current drawing session
            currentDrawing = null;

            // Invoke the OnEndDrawing event
            OnEndDrawing?.Invoke();
        }
        // Switch the pen's color if the Button on the Oculus controller is pressed
        else if (OVRInput.GetDown(OVRInput.Button.One))
        {
            SwitchColor();
        }
    }

    private void Draw()
    {
        // If no line is currently being drawn
        if (currentDrawing == null)
        {
            // Start from the first point of the line
            index = 0;

            // Prepare a new line to be drawn
            currentDrawing = CreateLineRenderer();

            // Set the first position of the new line to be the tip of the pen
            currentDrawing.SetPosition(0, tip.position);
        }
        else // If a line is already being drawn
        {
            // Check where the last point of the line is placed
            var currentPos = currentDrawing.GetPosition(index);

            // Check if the pen has been moved significantly from the last point
            if (Vector3.Distance(currentPos, tip.position) > 0.01f)
            {
                // Prepare to add a new point to the line
                index++;

                // Update the line with the new point
                currentDrawing.positionCount = index + 1;

                // Add the new point at the current position of the pen's tip
                currentDrawing.SetPosition(index, tip.position);
            }
        }
    }

    private LineRenderer CreateLineRenderer()
    {
        // GameObject to hold the LineRenderer
        GameObject lineObj = new GameObject("Line");

        // Add and configure the LineRenderer component
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = drawingMaterial;
        lineRenderer.startColor = lineRenderer.endColor = penColors[currentColorIndex];
        lineRenderer.startWidth = lineRenderer.endWidth = penWidth;
        lineRenderer.positionCount = 1;
        return lineRenderer;
    }

    private void SwitchColor()
    {
        // Cycle through the colors in penColors and update the pen tip color.
        currentColorIndex = (currentColorIndex + 1) % penColors.Length;
        SetTipColor();
        OnSwitchColor?.Invoke(penColors[currentColorIndex]);
    }

    private void SetTipColor()
    {
        // Set the color of the pen tip's material to the current color.
        if (tipMaterial != null)
        {
            tipMaterial.color = penColors[currentColorIndex];
        }
        else
        {
            Debug.LogError("Tip material not assigned!");
        }
    }
}