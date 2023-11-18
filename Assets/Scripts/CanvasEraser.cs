using System.Linq;
using Canvas;
using UnityEngine;

public class CanvasEraser : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _eraserSize = 5;

    private Renderer _renderer;
    private Color _clearColor = Color.white; 
    private Color[] _clearColors;
    private float _tipHeight;

    private RaycastHit _touch;
    private CanvasScript canvasScript;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;

    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();
        _clearColors = Enumerable.Repeat(_clearColor, _eraserSize * _eraserSize).ToArray();
        _tipHeight = _tip.localScale.y;
    }

    void Update()
    {
        Erase();
    }

    private void Erase()
    {
        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Canvas"))
            {
                if (canvasScript == null)
                {
                    canvasScript = _touch.transform.GetComponent<CanvasScript>();
                }

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * canvasScript.textureSize.x - (_eraserSize / 2));
                var y = (int)(_touchPos.y * canvasScript.textureSize.y - (_eraserSize / 2));

                if (y < 0 || y > canvasScript.textureSize.y || x < 0 || x > canvasScript.textureSize.x) return;

                if (_touchedLastFrame)
                {
                    canvasScript.texture.SetPixels(x, y, _eraserSize, _eraserSize, _clearColors);

                    for (float f = 0.01f; f < 1.00f; f += 0.01f)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        canvasScript.texture.SetPixels(lerpX, lerpY, _eraserSize, _eraserSize, _clearColors);
                    }

                    canvasScript.texture.Apply();
                }

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        canvasScript = null;
        _touchedLastFrame = false;
    }
}
