using System.Linq;
using Canvas;
using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasMarker : MonoBehaviour
{
   [SerializeField] private Transform _tip;
   [SerializeField] private int _penSize = 5;
   [SerializeField] private Color[] penColors; 

   private Renderer _renderer;
   private Color[] _colors;
   private float _tipHeight;

   private RaycastHit _touch;
   private CanvasScript canvasScript;
   private Vector2 _touchPos, _lastTouchPos;
   private bool _touchedLastFrame;
   private Quaternion _lastTouchRot;
   private int currentColorIndex = 0;

   
   public InputActionReference colorChangeAction;

   void Start()
   {
       _renderer = _tip.GetComponent<Renderer>();
       _tipHeight = _tip.localScale.y;
       // Initialize with the first color
       SetColor(penColors[currentColorIndex]); 
       
       colorChangeAction.action.performed += OnColorChangeActionPerformed;
       colorChangeAction.action.Enable();
   }

   void OnDestroy()
   {
       colorChangeAction.action.performed -= OnColorChangeActionPerformed;
   }

   void Update()
   {
       ProcessDrawing();
   }

   private void OnColorChangeActionPerformed(InputAction.CallbackContext context)
   {
       SwitchColor();
   }

   private void SwitchColor()
   {
       currentColorIndex = (currentColorIndex + 1) % penColors.Length;
       SetColor(penColors[currentColorIndex]);
   }

   public void SetColor(Color newColor)
   {
       // Change the material color of the tip
       _renderer.material.color = newColor; 
       // Update the color
       _colors = Enumerable.Repeat(newColor, _penSize * _penSize).ToArray(); 
   }
   private void ProcessDrawing()
   {
       if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight) && _touch.transform.CompareTag("Canvas"))
       {
           HandleCanvasTouch();
       }
       else
       {
           _touchedLastFrame = false;
           canvasScript = null;
       }
   }

   private void HandleCanvasTouch()
   {
       if (canvasScript == null)
       {
           canvasScript = _touch.transform.GetComponent<CanvasScript>();
       }

       _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

       var x = (int)(_touchPos.x * canvasScript.textureSize.x - (_penSize / 2));
       var y = (int)(_touchPos.y * canvasScript.textureSize.y - (_penSize / 2));

       if (x < 0 || x >= canvasScript.textureSize.x || y < 0 || y >= canvasScript.textureSize.y) return;

       DrawPixels(x, y);

       _lastTouchPos = new Vector2(x, y);
       _lastTouchRot = transform.rotation;
       _touchedLastFrame = true;
   }

   private void DrawPixels(int x, int y)
   {
       canvasScript.texture.SetPixels(x, y, _penSize, _penSize, _colors);

       if (_touchedLastFrame)
       {
           for (float f = 0.01f; f < 1.00f; f += 0.01f)
           {
               var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
               var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
               canvasScript.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);
           }
       }
      
       canvasScript.texture.Apply();
   }
}
